namespace TestApp
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using CustomSelectFileDlg;
    using CustomSelectFileDlg.EventArgs;
    using Microsoft.VisualBasic;
    using static Vanara.PInvoke.Shell32;

    public partial class Form1 : Form
    {
        private static readonly string RootPath = @"C:\";
        private static readonly IEnumerable<Entry> RootEntries = 
            DriveInfo.GetDrives().Select(d => 
                new Entry(d.Name) { Icon = Properties.Resources.drive });

        public Form1() => InitializeComponent();

        private static readonly ConcurrentDictionary<string, Bitmap?>
            IconCache = new();

        public static Bitmap? GetIcon(string file)
        {
            var extension = Path.GetExtension(file);
            if (IconCache.TryGetValue(extension, out var icon))
            {
                return icon;
            }
            icon = Icon.ExtractAssociatedIcon(file)?.ToBitmap();
            _ = IconCache.TryAdd(extension, icon);
            return icon;
        }

        public static string? GetMimeType(string file)
        {
            try
            {
                SHFILEINFO fileInfo = new();
                if (SHGetFileInfo(
                        file,
                        FileAttributes.Normal,
                        ref fileInfo,
                        SHFILEINFO.Size,
                        SHGFI.SHGFI_TYPENAME | SHGFI.SHGFI_USEFILEATTRIBUTES) !=
                    IntPtr.Zero)
                {
                    return fileInfo.szTypeName;
                }
            }
            catch (ArgumentException) { }

            return null;
        }

        private Entry ToEntry(string path)
        {
            var attr = File.GetAttributes(path);
            var info = new FileInfo(path);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                return new Entry(path.Length <= 3 ? path : Path.GetFileName(path))
                {
                    Size = null,
                    DateModified = info.LastWriteTimeUtc,
                    MimeType = "File folder",
                    Type = EntryType.Folder,
                };
            }

            return new Entry(Path.GetFileName(path))
            {
                Icon = GetIcon(path),
                Size = info.Length,
                DateModified = info.LastWriteTimeUtc,
                MimeType = GetMimeType(path),
                Type = EntryType.File,
            };
        }

        private static IEnumerable<Entry> GetShares(string server) =>
            new Vanara.SharedDevices(server)
                .Where(kvp => !kvp.Value.IsSpecial && kvp.Value.IsDiskVolume)
                .Select(kvp => new Entry(kvp.Key) { Type = EntryType.Folder });

        private static IEnumerable<string>? GetExtensionsFromFilter(string? filter)
        {
            if (filter is null)
            {
                return null;
            }
            var start = filter.LastIndexOf('(');
            if (start == -1 || start == filter.Length - 1)
            {
                return null;
            }
            var end = filter.LastIndexOf(')');
            if (end == -1)
            {
                return null;
            }

            var extensions = filter.Substring(start + 1, end - start - 1);
            return extensions.Split(';').Select(e => e.Trim());
        }

        private static IEnumerable<string> EnumerateFiles(string path, string? filter)
        {
            var extensions = GetExtensionsFromFilter(filter);
            if (extensions is null || !extensions.Any())
            {
                return Directory.EnumerateFiles(path);
            }
            return extensions
                .SelectMany(e => Directory.EnumerateFiles(path, e))
                .Distinct();
        }

        private IEnumerable<Entry> GetFolderContent(
            string path,
            RequestedEntryType requestedEntryType,
            string? filter)
        {
            if (!Directory.Exists(path))
            {
                if (!Uri.TryCreate(path, UriKind.Absolute, out var uri)
                    || !uri.IsUnc
                    || path.Trim('\\').Contains('\\'))
                {
                    throw new FileNotFoundException();
                }

                // If it's a UNC path to server, without subfolder, we show the
                // shares on that server.
                return GetShares(path);
            }

            // Add a backslash at the end to make sure we never try to get
            // the content of the current drive without a backslash or slash
            // at the end. If the current working directory is "d:\subfolder"
            // and we try to get the file system entries from "d:", we would
            // get the file system entries from "d:\subfolder" instead.
            // Adding the backslash prevents that.
            path += "\\";
            try
            {
                if (requestedEntryType == RequestedEntryType.Folders)
                {
                    return Directory.EnumerateDirectories(path).Select(ToEntry);
                }
                else if (requestedEntryType == RequestedEntryType.Files)
                {
                    return EnumerateFiles(path, filter)
                        .Select(ToEntry);
                }
                return EnumerateFiles(path, filter)
                    .Union(Directory.EnumerateDirectories(path))
                    .Select(ToEntry);
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<Entry>();
            }
        }

        private void HandleContentRequest(object? sender, ContentRequestedEventArgs e)
        {
            Debug.Print("Content Requested");
            if (sender is not CustomSelectFileDialog dialog)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(e.Path))
            {
                Debug.Print($"Path is null or white space, setting it back to {RootPath}");
                dialog.CurrentPath = RootPath;
                return;
            }
            try
            {
                e.Entries = GetFolderContent(e.Path, e.RequestedEntryType, e.SelectedFilter);
            }
            catch (FileNotFoundException)
            {
                dialog.CurrentPath = Path.GetDirectoryName(e.Path);
            }
        }

        private void SelectFolderDialog_Click(object sender, EventArgs e)
        {
            using (CustomSelectFileDialog dlg = new()
            {
                Text = "Select folder dialog",
                CurrentPath = SelectFolderDialogResult.Text,
                EntryIconStyle = IconStyle.FallbackToSimpleIcons,
                IsFolderSelector = true,
                ButtonUpEnabled = ButtonUpEnabledWhen.Always,
                RootFolders = RootEntries,
            })
            {
                dlg.ContentRequested += HandleContentRequest;
                dlg.PathSelected += (s, e) =>
                {
                    if (!Directory.Exists(e.Path))
                    {
                        MessageBox.Show($"Directory {e.Path} doesn't exists.");
                        e.IsValid = false;
                    }
                };

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SelectFolderDialogResult.Text = dlg.SelectedPath;
                }
            }
        }

        private void SelectFileDialog_Click(object sender, EventArgs e)
        {
            using (CustomSelectFileDialog dlg = new()
            {
                Text = "Select file dialog",
                Filter = new List<string> { "Text Files (*.txt)", "All Files (*.*)", "Binary Files (*.dll; *.exe)" },
                CurrentPath = SelectFileDialogResult.Text,
                EntryIconStyle = IconStyle.FallbackToSimpleIcons,
                IsFolderSelector = false,
                ButtonUpEnabled = ButtonUpEnabledWhen.Always,
                RootFolders = RootEntries,
            })
            {
                dlg.ContentRequested += HandleContentRequest;
                dlg.PathSelected += (s, e) =>
                {
                    if (!File.Exists(e.Path))
                    {
                        MessageBox.Show($"File {e.Path} doesn't exist");
                        e.IsValid = false;
                    }
                };

                while (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (!File.Exists(dlg.SelectedPath))
                    {
                        continue;
                    }
                    SelectFileDialogResult.Text = dlg.SelectedPath;
                    return;
                }
            }
        }

        private void SaveFileDialog_Click(object sender, EventArgs e)
        {
            using (CustomSelectFileDialog dlg = new()
            {
                Text = "Save file dialog",
                CurrentPath = SaveFileDialogResult.Text,
                EntryIconStyle = IconStyle.FallbackToSimpleIcons,
                IsFolderSelector = false,
                ButtonUpEnabled = ButtonUpEnabledWhen.Always,
                RootFolders = RootEntries,
            })
            {
                dlg.ContentRequested += HandleContentRequest;
                dlg.PathSelected += (s, e) =>
                {
                    if (File.Exists(e.Path))
                    {
                        if (MessageBox.Show(
                                "Override?",
                                "Override?",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
                            != DialogResult.Yes)
                        {
                            e.IsValid = false;
                        }
                        return;
                    }

                    var dirPath = Path.GetDirectoryName(e.Path);
                    if (!Directory.Exists(dirPath))
                    {
                        MessageBox.Show($"Can't create file. Directory {dirPath} doesn't exists.");
                        e.IsValid = false;
                    }
                };

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SaveFileDialogResult.Text = dlg.SelectedPath;
                }
            }
        }
    }
}