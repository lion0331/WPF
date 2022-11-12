using System;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace ManagedUIKitWPF
{
    public partial class MainView : Window
    {
        //// API Delegates
        delegate bool LZ4_Compress_File_Ptr(string filename);
        delegate bool LZ4_Decompress_File_Ptr(string filename, long filesize);

        //// Ported Functions
        LZ4_Compress_File_Ptr LZ4_Compress_File;
        LZ4_Decompress_File_Ptr LZ4_Decompress_File;

        void PrintLog(string log_input)
        {
            LogView.Text += $"[{DateTime.Now.ToShortTimeString()}] :: {log_input}\r\n";
            LogView.ScrollToEnd();
        }

        public MainView(IntPtr api_1_ptr, IntPtr api_2_ptr)
        {
            InitializeComponent();
            PrintLog("Initializing ...");

            //// Recovering Native Functions
            if (api_1_ptr != IntPtr.Zero && api_2_ptr != IntPtr.Zero)
            {
                PrintLog("Assigning Pointers to Functions...");
                LZ4_Compress_File = (LZ4_Compress_File_Ptr)Marshal.GetDelegateForFunctionPointer(api_1_ptr, typeof(LZ4_Compress_File_Ptr));
                LZ4_Decompress_File = (LZ4_Decompress_File_Ptr)Marshal.GetDelegateForFunctionPointer(api_2_ptr, typeof(LZ4_Decompress_File_Ptr));
                PrintLog("Functions has been assigned successfully.");
                LogView.TextAlignment = TextAlignment.Left;
                LogView.VerticalContentAlignment = VerticalAlignment.Top;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PrintLog("WPF UI has been initialized successfully.");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FileList.Items.Add(openFileDialog.FileName);
                PrintLog($"File '{Path.GetFileName(openFileDialog.FileName)}' has been added to list.");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string[] get_files = Directory.GetFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories);
                    for (int i = 0; i < get_files.Length; i++)
                    {
                        FileList.Items.Add(get_files[i]);
                    }

                    PrintLog($"{get_files.Length} files has been added.");
                }

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FileList.Items.Clear();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                long length = new System.IO.FileInfo(FileList.SelectedItem.ToString()).Length;
                PrintLog($"File '{Path.GetFileName(FileList.SelectedItem.ToString())}' Size is : {length} bytes");
                Orig_Size.Text = length.ToString();
            }
            catch (Exception err) { PrintLog($"ERROR : {err.Message}"); }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < FileList.Items.Count; i++)
            {
                string file_path = (FileList.Items[i]).ToString();
                PrintLog($"Compressing '{Path.GetFileName(file_path)}'...");
                try
                {
                    bool compression_process = LZ4_Compress_File(file_path);
                    if (compression_process)
                    {
                        PrintLog($"File '{Path.GetFileName(file_path)}' has been compressed successfully!");
                    }
                    else
                    {
                        PrintLog($"File '{Path.GetFileName(file_path)}' compression failed.");
                    }
                }
                catch
                {
                    PrintLog($"File '{Path.GetFileName(file_path)}' compression failed with fatal error.");
                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            string file_path = FileList.SelectedItem.ToString();
            if (file_path.Contains("_.lz4"))
            {
                try
                {
                    long file_orig_size = Convert.ToInt64(Orig_Size.Text);
                    bool compression_process = LZ4_Decompress_File(file_path, file_orig_size);
                    if (compression_process)
                    {
                        PrintLog($"File '{Path.GetFileName(file_path)}' has been decompressed successfully!");
                    }
                    else
                    {
                        PrintLog($"File '{Path.GetFileName(file_path)}' decompression failed.");
                    }
                }
                catch
                {
                    PrintLog($"File '{Path.GetFileName(file_path)}' decompression failed with fatal error , Check original size input.");
                }
            }
            else
            {
                PrintLog($"File '{Path.GetFileName(file_path)}' is not a valid LZ4 compressed file.");
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            LogView.Text = "";
        }
    }
}