﻿using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using Skrypt.Engine;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.Win32;
using System.ComponentModel;

namespace SkryptEditor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SkryptEngine _engine;
        string _documentPath;

        public MainWindow() {
            InitializeComponent();

            _engine = new SkryptEngine();

            //_outputter = new Output.TextBoxOutputter(Terminal);
            //Console.SetOut(_outputter);

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"..\..\SkryptHighlighting.xml");

            using (XmlTextReader reader = new XmlTextReader(new StreamReader(path))) {
                textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            try {
                _documentPath = Serializing.ReadFromBinaryFile<string>(Serializing.AssemblyDirectory + "/lastDocument.txt");
                textEditor.Text = File.ReadAllText(_documentPath);
            } catch {
                _documentPath = string.Empty;
            }

            RoutedCommand newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCmd, OnSave));

            newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCmd, OnNew));

            newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCmd, OnRun));
        }

        private void SaveFile () {
            if (_documentPath == string.Empty) {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog {
                    InitialDirectory = "c:\\",
                    Filter = "skt files (*.skt)|*.skt|All files (*.*)|*.*",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                saveFileDialog1.ShowDialog();
                _documentPath = saveFileDialog1.FileName;
            }

            if (_documentPath != string.Empty) {
                Console.WriteLine("Saving to " + _documentPath);

                File.WriteAllText(_documentPath, textEditor.Text);
            }
        }

        private void OpenFile() {
            OpenFileDialog openFileDialog1 = new OpenFileDialog {
                InitialDirectory = _documentPath,
                Filter = "skt files (*.skt)|*.skt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName != string.Empty) {
                _documentPath = openFileDialog1.FileName;

                if (_documentPath != string.Empty)
                    textEditor.Text = File.ReadAllText(_documentPath);
            }
        }


        private void OnNew(object sender, RoutedEventArgs e) {
            textEditor.Text = string.Empty;
            _documentPath = string.Empty;
        }

        private void OnExit(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnRun(object sender, RoutedEventArgs e) {
            Console.Clear();

            if (_documentPath != string.Empty)
                File.WriteAllText(_documentPath, textEditor.Text);

            var engine = new SkryptEngine();
            engine.AddType(typeof(Canvas.Drawing));
            var code = textEditor.Text;

            try {
                engine.Parse(code);
            }
            catch (Exception exception) {

                if (exception.GetType() == typeof(SkryptException)) {
                    if (((SkryptException)exception).Token != null)
                        textEditor.CaretOffset = ((SkryptException)exception).Token.Start;
                }
            }
        }

        private void OnSave(object sender, RoutedEventArgs e) {
            SaveFile();
        }

        private void OnOpen(object sender, RoutedEventArgs e) {
            OpenFile();
        }

        private void Window_Exit(object sender, CancelEventArgs e) {
            if (_documentPath != string.Empty)
                File.WriteAllText(_documentPath, textEditor.Text);

            Serializing.WriteToBinaryFile<string>(Serializing.AssemblyDirectory + "/lastDocument.txt", _documentPath);
        }
    }
}
