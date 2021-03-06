﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncSpotlightImages {
    class Program {
        static void Main(string[] args) {

            var sourceDir = @"C:\Users\gert\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";
            var destDir = @"C:\Users\gert\OneDrive\Wallpapers";

            // Remove old files
            var currentFiles = Directory.GetFiles(destDir).Select(s => new FileInfo(s));
            foreach(var current in currentFiles) {
                if (current.Name.Contains(".archive.")) continue;
                // Check if counterpart exists
                var orig = Path.Combine(sourceDir, Path.GetFileNameWithoutExtension(current.Name));
                if (!File.Exists(orig)) {
                    if (current.Name.Contains(".junk")) {
                        // Console.WriteLine($"Remove old file - {current.Name}");
                        current.Delete();
                    } else if (!current.Name.Contains(".archive.")) {
                        // Rename to archive
                        Console.WriteLine($"Archive - {current.Name}");
                        var newName = Path.ChangeExtension(current.FullName, ".archive.jpg");
                        if (File.Exists(newName)) File.Delete(newName);
                        current.MoveTo(Path.ChangeExtension(current.FullName, ".archive.jpg"));
                    }
                }
            }

            // Copy new files
            var sourceFiles = Directory.GetFiles(sourceDir).Select(s => new FileInfo(s));
            foreach(var sourceFile in sourceFiles) {
                // see if already exists
                if (File.Exists(Path.Combine(destDir, sourceFile.Name + ".jpg"))) continue;
                if (File.Exists(Path.Combine(destDir, sourceFile.Name + ".junk"))) continue;

                // See if file is jpg
                try {
                    using (var jpg = Bitmap.FromFile(sourceFile.FullName)) {
                        Console.WriteLine($"Found new image - {sourceFile.Name}");
                        if (jpg.Width < 480) {
                            CreateJunkFile(destDir, sourceFile);
                            continue;
                        }
                        if (jpg.Height > jpg.Width) {
                            CreateJunkFile(destDir, sourceFile);
                            continue;
                        }
                    }
                    sourceFile.CopyTo(Path.Combine(destDir, sourceFile.Name + ".jpg"));
                } catch (Exception e) {
                    // Not a jpg
                    CreateJunkFile(destDir, sourceFile);
                }
            }

        }

        static void CreateJunkFile(string folder, FileInfo source) {
            var path = Path.Combine(folder, source.Name + ".junk");
            using (var f = File.Create(path)) {
                f.WriteByte(0);
            }
        }
    }
}
