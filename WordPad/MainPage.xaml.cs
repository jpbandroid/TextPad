﻿using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WordPad.WordPadUI;
using WordPad.Helpers;
using Windows.Storage.Provider;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Graphics.Printing;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Email;
using WordPad.WordPadUI.Settings;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Data;
using Windows.Graphics.Display;
using Windows.ApplicationModel.Resources.Core;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using Windows.UI.Xaml.Documents;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using CheckBox = Windows.UI.Xaml.Controls.CheckBox;
using Application = Windows.UI.Xaml.Application;

// RectifyPad made by Lixkote with help of some others for Rectify11.
// Main page c# source code.

namespace RectifyPad
{

    public class ZoomConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double zoom)
            {
                return $"{zoom * 100}%";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string text && text.EndsWith("%"))
            {
                if (double.TryParse(text.TrimEnd('%'), out double zoom))
                {
                    return zoom / 100;
                }
            }
            return value;
        }
    }
    public class HalfValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double number)
            {
                return number / 2;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double number)
            {
                return number * 2;
            }
            return value;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private bool saved = true;

        public bool _wasOpen = false;
        private string appTitleStr => "RectifyPad";

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private bool updateFontFormat = true;
        public string ApplicationName => "RectifyPad";
        public string ZoomString => ZoomSlider.Value.ToString() + "%";

        private string fileNameWithPath = "";

        string originalDocText = "";

        public List<string> Fonts
        {
            get
            {
                return CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();
            }
        }

        public ObservableCollection<double> ZoomOptions { get; } = new ObservableCollection<double> { 5, 4, 3, 2, 1, 0.75, 0.5, 0.25, 0.125 };

        public List<double> FontSizes { get; } = new List<double>()
            {
                8,
                9,
                10,
                11,
                12,
                14,
                16,
                18,
                20,
                24,
                28,
                36,
                48,
                72
            };


        public MainPage()
        {
            // Enable navigation cache
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            // Run the startup voids
            InitializeComponent();
            CheckFirstRun();
            LoadThemeFromSettings();
            LoadSettingsValues();
            PopulateRecents();
            ConnectRibbonToolbars();

            // Connect the events and others
            TextRuler.LeftHangingIndentChanging += TextRuler_LeftHangingIndentChanging;
            TextRuler.LeftIndentChanging += TextRuler_LeftIndentChanging;
            TextRuler.RightIndentChanging += TextRuler_RightIndentChanging;
            TextRuler.BothLeftIndentsChanged += TextRuler_BothLeftIndentsChanged;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
            ribbongrid.DataContext = this;

        }

        private void CheckFirstRun()
        {
            /// This function is responsible for applying the default settings on the app's first startup.
            /// Modifying these will change the default settings of the application:            


            // Get the default resource context for the app
            ResourceContext defaultContext = ResourceContext.GetForCurrentView();

            // Set the default settings:
            string marginUnit = "Inches";
            string fontName = "Calibri";
            string fontSize = "11";
            string theme = "Light";
            string papersize = "A4";
            string papersource = "Auto";
            string pagesetupBmargin = "1.25";
            string pagesetupRmargin = "1.25";
            string pagesetupTmargin = "1";
            string pagesetupLmargin = "1";
            string isprintpagenumbers = "no";
            string orientation = "Portrait";
            string indentationL = "0";
            string indentationR = "0";
            string indentationFL = "0";
            string linespacing = "1,0";
            string is10ptenabled = "no";
            string alignment = "Left";
            string textwrapping = "wrapruler";
            // Get the local settings
            var localSettings = ApplicationData.Current.LocalSettings;

            // Check if the app has been launched before and if not, set the default settings.
            if (localSettings.Values["FirstRun"] == null)
            {
                // Set the settings values to the default ones
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["unitSetting"] = marginUnit;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["themeSetting"] = theme;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["fontfamilySetting"] = fontName;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["fontSizeSetting"] = fontSize;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["papersize"] = papersize;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["papersource"] = papersource;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["pagesetupBmargin"] = pagesetupBmargin;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["pagesetupRmargin"] = pagesetupRmargin;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["pagesetupTmargin"] = pagesetupTmargin;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["pagesetupLmargin"] = pagesetupLmargin;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["isprintpagenumbers"] = isprintpagenumbers;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["indentationL"] = indentationL;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["indentationR"] = indentationR;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["indentationFL"] = indentationFL;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["is10ptenabled"] = is10ptenabled;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["alignment"] = alignment;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["orientation"] = orientation;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["linespacing"] = linespacing;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["textwrapping"] = textwrapping;

                // Set the value to indicate that the app has been launched
                localSettings.Values["FirstRun"] = false;
            }
        }

        private void LoadSettingsValues()
        {
            try
            {
                // Load text wrapping value from settings:
                string textWrapping = localSettings.Values["textwrapping"] as string;
                if (textWrapping == "wrapwindow")
                {
                    Editor.TextWrapping = TextWrapping.Wrap;
                }
                else if (textWrapping == "nowrap")
                {
                    Editor.TextWrapping = TextWrapping.NoWrap;
                }
                else if (textWrapping == "wrapruler")
                {
                    // Add a function here that will do the ruler-based wrapping
                }

                // Load margin values from the settings:
                var settings = ApplicationData.Current.LocalSettings;

                string unit = settings.Values["unitSetting"] as string;
                string Lmargin = settings.Values["pagesetupLmargin"] as string;
                string Rmargin = settings.Values["pagesetupRmargin"] as string;
                string Tmargin = settings.Values["pagesetupTmargin"] as string;
                string Bmargin = settings.Values["pagesetupBmargin"] as string;

                // Debugging output to check retrieved values and their types
                Debug.WriteLine($"unit: {unit}, Lmargin: {Lmargin}, Rmargin: {Rmargin}, Tmargin: {Tmargin}, Bmargin: {Bmargin}");

                // Check if any of the values retrieved are null or not of type string
                if (!string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(Lmargin) && !string.IsNullOrEmpty(Rmargin) && !string.IsNullOrEmpty(Tmargin) && !string.IsNullOrEmpty(Bmargin))
                {
                    // Convert margin values to match the unit and format them as needed
                    double left = ConvertToUnitAndFormat(Lmargin, unit);
                    double right = ConvertToUnitAndFormat(Rmargin, unit);
                    double top = ConvertToUnitAndFormat(Tmargin, unit);
                    double bottom = ConvertToUnitAndFormat(Bmargin, unit);

                    Editor.Margin = new Thickness(left, top, right, bottom);
                }
                else
                {
                    // Handle the case where one or more values are missing or not of type string
                    Debug.WriteLine("One or more settings values are missing or not of type string.");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                Debug.WriteLine($"An exception occurred: {ex.Message}");
            }
        }


        private double ConvertFromUnit(double value, string unit)
        {
            switch (unit)
            {
                case "Inches":
                    return value;
                case "Centimeters":
                    return value / 2.54; // Convert centimeters to inches
                case "Points":
                    return value / 72; // Convert points to inches
                case "Picas":
                    return value / 6; // Convert picas to inches
                default:
                    return value; // Default to inches
            }
        }

        private double ConvertToUnitAndFormat(string value, string unit)
        {
            if (double.TryParse(value, out double margin))
            {
                // Convert margin values to inches
                margin = ConvertToUnit(margin, unit);

                // Limit the margin value
                double maxMargin = 100.0; // Set a maximum margin value
                margin = Math.Min(maxMargin, margin);

                // Format the margin value as needed
                string formattedMargin = margin.ToString("0.##"); // Display with up to 2 decimal places
                return double.Parse(formattedMargin);
            }
            else
            {
                // Handle the case where the input value is not a valid number
                Debug.WriteLine($"Invalid numeric value: {value}");
                return 0.0; // or some default value
            }
        }




        private void LoadThemeFromSettings()
        {
            string value = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["themeSetting"];
            if (value != null)
            {
                try
                {
                    // Change title bar color if needed
                    ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                    if (value == "Dark")
                    {
                        titleBar.ButtonForegroundColor = Colors.White;
                        App.RootTheme = ElementTheme.Dark;
                    }
                    else if (value == "Light")
                    {
                        titleBar.ButtonForegroundColor = Colors.Black;
                        App.RootTheme = ElementTheme.Light;
                    }
                    else
                    {
                        App.RootTheme = ElementTheme.Default;
                        if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                        {
                            titleBar.ButtonForegroundColor = Colors.White;
                        }
                        else
                        {
                            titleBar.ButtonForegroundColor = Colors.Black;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    Debug.WriteLine($"An exception occurred: {ex.Message}");
                }

            }
            Window.Current.SetTitleBar(AppTitleBar);
        }

        private void TextRuler_LeftIndentChanging(int NewValue)
        {
            try
            {
                var paragraph = Editor.Document.Selection.ParagraphFormat;
                if (paragraph != null)
                {
                    var textIndentInMillimeters = TextRuler.LeftIndent;
                    var textIndentInPixels = ConvertDipsToPixels(textIndentInMillimeters);
                    paragraph.SetIndents((float)textIndentInPixels, 0, 0);
                }
            }
            catch (Exception)
            {

            }
        }



        private double ConvertDipsToPixels(double dips)
        {
            var dpiFactor = DisplayInformation.GetForCurrentView().LogicalDpi / 33; // Convert from DIPs to physical pixels
            return dips * dpiFactor;
        }

        private void TextRuler_LeftHangingIndentChanging(int NewValue)
        {
            try
            {
                var paragraph = Editor.Document.Selection.ParagraphFormat;
                if (paragraph != null)
                {
                    var textIndentInMillimeters = TextRuler.LeftIndent;
                    var hangingIndentInMillimeters = TextRuler.LeftHangingIndent;
                    var textIndentInPixels = ConvertDipsToPixels(textIndentInMillimeters);
                    var hangingIndentInPixels = ConvertDipsToPixels(hangingIndentInMillimeters);
                    Editor.Document.Selection.ParagraphFormat.SetIndents((float)textIndentInPixels, (float)hangingIndentInPixels, 0);
                    // Set the margins for the RichEditBox
                    Editor.Margin = new Thickness((float)textIndentInPixels, (float)hangingIndentInPixels, 0, 0);
                }
            }
            catch (Exception)
            {

            }
        }
        private void TextRuler_BothLeftIndentsChanged(int leftIndent, int hangIndent)
        {
            var paragraph = Editor.Document.Selection.ParagraphFormat;
            if (paragraph != null)
            {
                double indentInMillimeters = leftIndent;
                double hangingIndentInMillimeters = hangIndent;

                double indentInPixels = ConvertDipsToPixels(indentInMillimeters);
                double hangingIndentInPixels = ConvertDipsToPixels(hangingIndentInMillimeters);

                Editor.Margin = new Thickness((float)indentInPixels, 0, (float)hangingIndentInPixels, 0);
            }
        }


        private void TextRuler_RightIndentChanging(int NewValue)
        {
            try
            {
                var paragraph = Editor.Document.Selection.ParagraphFormat;
                if (paragraph != null)
                {
                    var rightIndentInMillimeters = TextRuler.RightIndent;
                    var rightIndentInPixels = ConvertDipsToPixels(rightIndentInMillimeters);
                    Editor.Margin = new Thickness(0, 0, -(float)rightIndentInPixels, 0);
                }
            }
            catch (Exception)
            {

            }
        }


        private void ConnectRibbonToolbars()
        {
            editribbontoolbar.Editor = Editor;
            insertribbontoolbar.Editor = Editor;
            pararibbontoolbar.Editor = Editor;
            fontribbontoolbar.Editor = Editor;

            //collapsed variants also need to be 'connected'
            editribbontoolbarcol.Editor = Editor;
            insertribbontoolbarcol.Editor = Editor;
            pararibbontoolbarcol.Editor = Editor;
            fontribbontoolbarcol.Editor = Editor;
        }

        private async void PopulateRecents()
        {
            var recentlyUsedItems = await RecentlyUsedHelper.GetRecentlyUsedItems();
            var recentItemsSubItem = RecentItemsSubItem;
            foreach (var item in recentlyUsedItems)
            {
                var menuItem = new MenuFlyoutItem { Text = item.Name };
                menuItem.Click += async (s, args) =>
                {
                    var file = await StorageFile.GetFileFromPathAsync(item.Path);
                    await RecentlyUsedHelper.AddToMostRecentlyUsedList(file);
                    // Open the file here
                };
                recentItemsSubItem.Items.Add(menuItem);
            }
        }

        private MarkerType _type = MarkerType.Bullet;

        public SvgImageSource cutimgthemed
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "Cut.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        public SvgImageSource zoomin
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "ZoomIn.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        public SvgImageSource zoomout
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "ZoomOut.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        public SvgImageSource printpreviewprint
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "Print.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        public SvgImageSource printpreviewzoomminus
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "ZoomOut.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        public SvgImageSource printpreviewzoomminusdis
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "ZoomOutDisabled.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        public SvgImageSource printpreviewzoomplusdis
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "ZoomInDisabled.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }
        public SvgImageSource printpreviewzoomplus
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "ZoomIn.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        public SvgImageSource pasteimgthemed
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "Paste.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        public SvgImageSource copyimgthemed
        {
            get
            {
                var theme = Application.Current.RequestedTheme;
                var folderName = theme == ApplicationTheme.Dark ? "theme-dark" : "theme-light";
                var imageName = "Copy.svg";
                var imagePath = $"ms-appx:///Assets/{folderName}/{imageName}";
                return new SvgImageSource(new Uri(imagePath));
            }
        }

        private void MyListButton_IsCheckedChanged(Microsoft.UI.Xaml.Controls.ToggleSplitButton sender, Microsoft.UI.Xaml.Controls.ToggleSplitButtonIsCheckedChangedEventArgs args)
        {
            if (sender.IsChecked)
            {
                //add bulleted list
                Editor.Document.Selection.ParagraphFormat.ListType = _type;
            }
            else
            {
                //remove bulleted list
                Editor.Document.Selection.ParagraphFormat.ListType = MarkerType.None;
            }
        }

        public class RtfConverter
        {
            private readonly Document _document;

            public RtfConverter(Document document)
            {
                _document = document;
            }

            public string ConvertToRtf()
            {
                var rtfWriter = new StringWriter();
                rtfWriter.WriteLine("{\\rtf1\\ansi\\deff0");

                // Define a color table with all possible RGB values
                rtfWriter.WriteLine("{\\colortbl ;");

                for (int r = 0; r <= 255; r++)
                {
                    for (int g = 0; g <= 255; g++)
                    {
                        for (int b = 0; b <= 255; b++)
                        {
                            rtfWriter.WriteLine($"\\red{r}\\green{g}\\blue{b};");
                        }
                    }
                }

                rtfWriter.WriteLine("}");

                foreach (var paragraph in _document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    rtfWriter.WriteLine("{\\pard");

                    foreach (var run in paragraph.Elements<Run>())
                    {
                        if (run.RunProperties != null)
                        {
                            if (run.RunProperties.Bold != null && run.RunProperties.Bold.Val)
                            {
                                rtfWriter.Write("\\b ");
                            }

                            if (run.RunProperties.Italic != null && run.RunProperties.Italic.Val)
                            {
                                rtfWriter.Write("\\i ");
                            }

                            if (run.RunProperties.Color != null)
                            {
                                var colorHex = run.RunProperties.Color.Val;
                                rtfWriter.Write($"\\cf{GetColorIndex(colorHex)} ");
                            }

                            if (run.RunProperties.FontSize != null)
                            {
                                var fontSize = run.RunProperties.FontSize.Val;
                                rtfWriter.Write($"\\fs{fontSize} ");
                            }
                        }

                        foreach (var text in run.Elements<Text>())
                        {
                            rtfWriter.Write(text.Text);
                        }

                        if (run.RunProperties != null && ((run.RunProperties.Bold != null && run.RunProperties.Bold.Val) ||
                            (run.RunProperties.Italic != null && run.RunProperties.Italic.Val)))
                        {
                            rtfWriter.Write("\\b0\\i0 ");
                        }
                    }

                    rtfWriter.WriteLine("}");
                }

                rtfWriter.WriteLine("}");

                return rtfWriter.ToString();
            }

            private int GetColorIndex(string colorHex)
            {
                // You can calculate the color index based on the RGB values in the color table
                // For simplicity, this example assumes that colorHex is in the format "RRGGBB"
                int red = Convert.ToInt32(colorHex.Substring(0, 2), 16);
                int green = Convert.ToInt32(colorHex.Substring(2, 2), 16);
                int blue = Convert.ToInt32(colorHex.Substring(4, 2), 16);

                // Calculate the color index
                int colorIndex = (red * 256 * 256 + green * 256 + blue) + 1;

                return colorIndex;
            }

        }

        private async System.Threading.Tasks.Task<string> LoadDocxAndConvertToRtf(StorageFile file)
        {
            using (var stream = await file.OpenStreamForReadAsync())
            {
                using (var doc = WordprocessingDocument.Open(stream, false))
                {
                    var converter = new RtfConverter(doc.MainDocumentPart.Document);
                    return converter.ConvertToRtf();
                }
            }
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            // Open a text file.
            FileOpenPicker open = new FileOpenPicker();
            open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".rtf");
            open.FileTypeFilter.Add(".txt");
            open.FileTypeFilter.Add(".odt");
            open.FileTypeFilter.Add(".docx");

            StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                string fileExtension = file.FileType.ToLower(); // Get the file extension in lowercase

                if (fileExtension == ".docx")
                {
                    // Load the DOCX file and convert it to RTF
                    var rtfText = await LoadDocxAndConvertToRtf(file);
                    if (!string.IsNullOrEmpty(rtfText))
                    {
                        // Get the RTF string from the TextBox
                        string rtfString = rtfText;

                        // Convert the RTF string to a byte array
                        byte[] rtfBytes = Encoding.UTF8.GetBytes(rtfString);

                        // Create a MemoryStream from the byte array
                        using (MemoryStream stream = new MemoryStream(rtfBytes))
                        {
                            // Create a RandomAccessStream from the MemoryStream
                            IRandomAccessStream randomAccessStream = stream.AsRandomAccessStream();

                            // Create a StorageFile to save the RTF content
                            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                            StorageFile rtfFile = await localFolder.CreateFileAsync("output.rtf", CreationCollisionOption.ReplaceExisting);

                            Editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randomAccessStream);
                        }
                    }
                }
                else if (fileExtension == ".rtf" || fileExtension == ".odt")
                {
                    // Handle other file types (e.g., .rtf, .txt, .odt) loading here
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        IBuffer buffer = await FileIO.ReadBufferAsync(file);
                        var reader = DataReader.FromBuffer(buffer);
                        reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                        string text = reader.ReadString(buffer.Length);
                        // Load the file into the Document property of the RichEditBox.
                        Editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                    }
                }
                else if (fileExtension == ".txt")
                {
                    // Handle other file types (e.g., .rtf, .txt, .odt) loading here
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        IBuffer buffer = await FileIO.ReadBufferAsync(file);
                        var reader = DataReader.FromBuffer(buffer);
                        reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                        string text = reader.ReadString(buffer.Length);
                        // Load the file into the Document property of the RichEditBox.
                        Editor.Document.LoadFromStream(TextSetOptions.None, randAccStream);
                    }
                }

                AppTitle.Text = file.Name + " - " + appTitleStr;
                fileNameWithPath = file.Path;

                saved = false;
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
            }
        }

        private void SubscriptButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.FormatSelected(RichEditHelpers.FormattingMode.Subscript);
        }

        private void SuperScriptButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.FormatSelected(RichEditHelpers.FormattingMode.Superscript);
        }
        private void StrikethroughButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.FormatSelected(RichEditHelpers.FormattingMode.Strikethrough);
        }


        private void NoneNumeral_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.ParagraphFormat.ListType = MarkerType.None;
            Editor.Focus(FocusState.Keyboard);
        }

        private void DottedNumeral_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.ParagraphFormat.ListType = MarkerType.Bullet;
            Editor.Focus(FocusState.Keyboard);
        }

        private void NumberNumeral_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.ParagraphFormat.ListType = MarkerType.Arabic;
            Editor.Focus(FocusState.Keyboard);
        }

        private void LetterSmallNumeral_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.ParagraphFormat.ListType = MarkerType.LowercaseEnglishLetter;
            Editor.Focus(FocusState.Keyboard);
        }

        private void LetterBigNumeral_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.ParagraphFormat.ListType = MarkerType.UppercaseEnglishLetter;
            Editor.Focus(FocusState.Keyboard);
        }

        private void SmalliNumeral_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.ParagraphFormat.ListType = MarkerType.LowercaseRoman;
            Editor.Focus(FocusState.Keyboard);
        }

        private void BigINumeral_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.ParagraphFormat.ListType = MarkerType.UppercaseRoman;
            Editor.Focus(FocusState.Keyboard);
        }


        private void AlignRightButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.AlignSelectedTo(RichEditHelpers.AlignMode.Right);
            editor_SelectionChanged(sender, e);
        }

        private void AlignCenterButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.AlignSelectedTo(RichEditHelpers.AlignMode.Center);
            editor_SelectionChanged(sender, e);
        }

        private void AlignLeftButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.AlignSelectedTo(RichEditHelpers.AlignMode.Left);
            editor_SelectionChanged(sender, e);
        }

        private void FindBoxRemoveHighlights()
        {
            ITextRange documentRange = Editor.Document.GetRange(0, TextConstants.MaxUnitCount);
            SolidColorBrush defaultBackground = Editor.Background as SolidColorBrush;
            SolidColorBrush defaultForeground = Editor.Foreground as SolidColorBrush;

            documentRange.CharacterFormat.BackgroundColor = defaultBackground.Color;
            documentRange.CharacterFormat.ForegroundColor = defaultForeground.Color;
        }

        private void RemoveHighlightButton_Click(object sender, RoutedEventArgs e)
        {
            FindBoxRemoveHighlights();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Redo();
        }
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.Cut();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.Copy();
        }

        private void Paste_Click(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
        {
            Editor.Document.Selection.Paste(0);
        }

        private void ZoomSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Animate the zoom factor from the old value to the new value
            AnimateZoomSecond(e.OldValue, e.NewValue);
        }


        private void EditorContentHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            /*
 
            The status bar is slightly tinted by the mica backdrop

            Clipping the editor is needed, as the editor has a
            shadow. Without the clip, the shadow would be visible
            on the status bar

            */

            RectangleGeometry rectangle = new RectangleGeometry();
            rectangle.Rect = new Rect(0, 0, EditorContentHost.ActualWidth, EditorContentHost.ActualHeight);
            EditorContentHost.Clip = rectangle;
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog dialog = new AboutDialog();
            await dialog.ShowAsync();

        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Editor != null)
            {
                Editor.Focus(FocusState.Programmatic);

                // Get the position of the last character in the RichEditBox
                int lastPosition = Editor.Document.Selection.EndPosition;

                // Set the selection range to the entire document
                Editor.Document.Selection.SetRange(0, lastPosition);
            }
        }

        private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            object value = Editor.Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
        }

        private void ToggleButton_Unchecked_1(object sender, RoutedEventArgs e)
        {
            object value = Editor.Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
        }

        private async Task ShowUnsavedDialog()
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            ContentDialog aboutDialog = new ContentDialog()
            {
                Title = "Do you want to save your work?",
                Content = "There are unsaved changes in " + '\u0022' + fileName + '\u0022',
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't save",

            };
            aboutDialog.DefaultButton = ContentDialogButton.Primary;
            ContentDialogResult result = await aboutDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SaveFile(true);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            }
        }

        private async Task ShowUnsavedDialogSE()
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            ContentDialog aboutDialog = new ContentDialog()
            {
                Title = "Do you want to save your work?",
                Content = "There are unsaved changes in " + '\u0022' + fileName + '\u0022',
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't save",

            };
            aboutDialog.DefaultButton = ContentDialogButton.Primary;
            ContentDialogResult result = await aboutDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SaveFile(true);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // Clear the current document.
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        private void ToggleButton_Checked_2(object sender, RoutedEventArgs e)
        {
            object value = Editor.Document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
        }

        private void ToggleButton_Checked_3(object sender, RoutedEventArgs e)
        {
            object value = Editor.Document.Selection.CharacterFormat.Strikethrough = FormatEffect.Toggle;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Editor.ChangeFontSize((float)2);
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(false);
        }


        private async void AddImageButton_Click(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
        {
            // Open an image file.
            FileOpenPicker open = new FileOpenPicker();
            open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".png");
            open.FileTypeFilter.Add(".jpg");
            open.FileTypeFilter.Add(".jpeg");

            StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.Read);
                var properties = await file.Properties.GetImagePropertiesAsync();
                int width = (int)properties.Width;
                int height = (int)properties.Height;

                // Load the file into the Document property of the RichEditBox.
                Editor.Document.Selection.InsertImage(width, height, 0, VerticalCharacterAlignment.Baseline, "img", randAccStream);
            }
        }

        private async void SaveFile(bool isCopy)
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            if (isCopy || fileName == "Document")
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Formatted Document  .rtf", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Text Document  .txt", new List<string>() { ".txt" });
                //  savePicker.FileTypeChoices.Add("OpenDocument Text   .odt", new List<string>() { ".odt" });
                savePicker.FileTypeChoices.Add("Office Open XML Document   .docx", new List<string>() { ".docx" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";


                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                        switch (file.FileType)
                        {
                            case ".rtf":
                                // RTF file, format for it
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                            case ".txt":
                                // TXT File, disable RTF formatting so that this is plain text
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                            case ".docx":
                                // TXT File, disable RTF formatting so that this is plain text
                                {

                                    randAccStream.Dispose();
                                }
                                break;
                        }


                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox =
                            new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    AppTitle.Text = file.Name + " - " + appTitleStr;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Document")
            {
                string path = fileNameWithPath.Replace("\\" + fileName, "");
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            if (file.Name.EndsWith(".txt"))
                            {
                                Editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                                randAccStream.Dispose();
                            }
                            else
                            {
                                Editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                                randAccStream.Dispose();
                            }


                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox =
                                new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        saved = true;
                        AppTitle.Text = file.Name + " - " + appTitleStr;
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                    }
                }
                catch (Exception)
                {
                    SaveFile(true);
                }
            }
        }

        private void CancelColor_Click(object sender, RoutedEventArgs e)
        {
        }

        private void fontbackgroundcolorsplitbutton_Click(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
        {
            // If you see this, remind me to look into the splitbutton color applying logic
        }

        private void fontcolorsplitbutton_Click(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
        {
            // If you see this, remind me to look into the splitbutton color applying logic
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.FormatSelected(RichEditHelpers.FormattingMode.Italic);
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.FormatSelected(RichEditHelpers.FormattingMode.Bold);
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.FormatSelected(RichEditHelpers.FormattingMode.Underline);
        }

        private void AlignAdjustedButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ParagraphButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the ParagraphDialog
            ParagraphDialog paragraphDialog = new ParagraphDialog();

            // Show the dialog and wait for the user's input
            ContentDialogResult result = await paragraphDialog.ShowAsync();

            // If the user clicked the OK button, adjust the properties of the RichEditBox
            if (result == ContentDialogResult.Primary)
            {
                // Get the values from the dialog's TextBoxes and ComboBoxes
                TextBox leftTextBox = (TextBox)paragraphDialog.FindName("LeftTextBox");
                TextBox rightTextBox = (TextBox)paragraphDialog.FindName("RightTextBox");
                TextBox firstLineTextBox = (TextBox)paragraphDialog.FindName("FirstLineTextBox");
                ComboBox lineSpacingComboBox = (ComboBox)paragraphDialog.FindName("LineSpacingComboBox");

                // Parse the values and set the properties of the RichEditBox
                double left = double.Parse(leftTextBox.Text);
                double right = double.Parse(rightTextBox.Text);
                double firstLine = double.Parse(firstLineTextBox.Text);
                double lineSpacing = double.Parse(lineSpacingComboBox.SelectedItem.ToString());

                Editor.Margin = new Thickness(left, 0, right, 0);
                Editor.Document.Selection.ParagraphFormat.SetIndents((float)firstLine, 0, 0);
                Editor.Document.Selection.ParagraphFormat.SetLineSpacing(Windows.UI.Text.LineSpacingRule.AtLeast, (float)lineSpacing);
            }
        }

        private void editor_SelectionChanged(object sender, RoutedEventArgs e)
        {

            if (Editor.Document.Selection.CharacterFormat.Size > 0)
            {
                //font size is negative when selection contains multiple font sizes
                //FontSizeBox. = Editor.Document.Selection.CharacterFormat.Size;
            }
            //prevent accidental font changes when selection contains multiple styles
            updateFontFormat = false;
            updateFontFormat = true;
            // Get a reference to the RichEditBox control
            RichEditBox richEditBox = Editor;
        }

        private async void Button_Click_3Async(object sender, RoutedEventArgs e)
        {
                
        }

        private void DecreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            // Get a reference to the RichEditBox control
            RichEditBox richEditBox = Editor;

            // Decrease the font size of the currently selected text by 2 points
            richEditBox.Document.Selection.CharacterFormat.Size -= 2;
        }

        private void IncreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            // Get a reference to the RichEditBox control
            RichEditBox richEditBox = Editor;

            // Increase the font size of the currently selected text by 2 points
            richEditBox.Document.Selection.CharacterFormat.Size += 2;
        }

        private async void Button_Click_4Async(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Insert current date and time";

            // Create a ListView for the user to select the date format
            ListView listView = new ListView();
            listView.SelectionMode = ListViewSelectionMode.Single;

            // Create a list of date formats to display in the ListView
            List<string> dateFormats = new List<string>();
            dateFormats.Add(DateTime.Now.ToString("dd.M.yyyy"));
            dateFormats.Add(DateTime.Now.ToString("M.dd.yyyy"));
            dateFormats.Add(DateTime.Now.ToString("dd MMM yyyy"));
            dateFormats.Add(DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            dateFormats.Add(DateTime.Now.ToString("dd MMMM yyyy"));
            dateFormats.Add(DateTime.Now.ToString("hh:mm:ss tt"));
            dateFormats.Add(DateTime.Now.ToString("HH:mm:ss"));
            dateFormats.Add(DateTime.Now.ToString("dddd, dd MMMM yyyy, HH:mm:ss"));
            dateFormats.Add(DateTime.Now.ToString("dd MMMM yyyy, HH:mm:ss"));
            dateFormats.Add(DateTime.Now.ToString("MMM dd, yyyy"));

            // Set the ItemsSource of the ListView to the list of date formats
            listView.ItemsSource = dateFormats;

            // Set the content of the ContentDialog to the ListView
            dialog.Content = listView;

            // Make the insert button colored
            dialog.DefaultButton = ContentDialogButton.Primary;

            // Add an "Insert" button to the ContentDialog
            dialog.PrimaryButtonText = "OK";
            dialog.PrimaryButtonClick += (s, args) =>
            {
                string selectedFormat = listView.SelectedItem as string;
                string formattedDate = dateFormats[listView.SelectedIndex];
                Editor.Document.Selection.Text = formattedDate;
            };

            // Add a "Cancel" button to the ContentDialog
            dialog.SecondaryButtonText = "Cancel";

            // Show the ContentDialog
            await dialog.ShowAsync();
        }

        private PrintHelper _printHelper;
        private DataTemplate customPrintTemplate;
        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            string value = string.Empty;
            _printHelper = new PrintHelper(EditorContentHost);
            var printHelperOptions = new PrintHelperOptions(true);
            printHelperOptions.Orientation = PrintOrientation.Default;
            await _printHelper.ShowPrintUIAsync("Print Document", printHelperOptions, true);
        }
        private void pintpreview_Click(object sender, RoutedEventArgs e)
        {
            ribbongrid.Visibility = Visibility.Collapsed;
            PrintPreviewRibbon.Visibility = Visibility.Visible;
        }
        private void closeprintpreviewclick(object sender, RoutedEventArgs e)
        {
            ribbongrid.Visibility = Visibility.Visible;
            PrintPreviewRibbon.Visibility = Visibility.Collapsed;
        }

        bool isTextChanged = false;
        private readonly bool isCopy;

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
            string textStart;
            Editor.Document.GetText(TextGetOptions.UseObjectText, out textStart);

            if (textStart == "")
            {
                saved = true;
            }
            else
            {
                saved = false;
            }

            SolidColorBrush highlightBackgroundColor = (SolidColorBrush)App.Current.Resources["TextControlBackgroundFocused"];
            Editor.Document.Selection.CharacterFormat.BackgroundColor = highlightBackgroundColor.Color;
        }

        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            if (saved == false) { e.Handled = true; await ShowUnsavedDialog(); }
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is StorageFile file)
            {
                using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    var reader = DataReader.FromBuffer(buffer);
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                    string text = reader.ReadString(buffer.Length);
                    // Load the file into the Document property of the RichEditBox.
                    Editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                    Editor.Document.GetText(TextGetOptions.UseObjectText, out originalDocText);
                    //editor.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, text);
                    fileNameWithPath = file.Path;
                }
                saved = true;
                fileNameWithPath = file.Path;
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
                _wasOpen = true;
            }
        }

        private async void SaveAsRTF_Click(object sender, RoutedEventArgs e)
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            if (isCopy || fileName == "Document")
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Formatted Document", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Text Document", new List<string>() { ".txt" });
                savePicker.FileTypeChoices.Add("OpenDocument Text", new List<string>() { ".odt" });
                savePicker.FileTypeChoices.Add("Office Open XML Document", new List<string>() { ".docx" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";


                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                        switch (file.Name.EndsWith(".txt"))
                        {
                            case false:
                                // RTF file, format for it
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                            case true:
                                // TXT File, disable RTF formatting so that this is plain text
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                        }


                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox =
                            new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    AppTitle.Text = file.Name + " - " + appTitleStr;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Document")
            {
                string path = fileNameWithPath.Replace("\\" + fileName, "");
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            if (file.Name.EndsWith(".txt"))
                            {
                                Editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                                randAccStream.Dispose();
                            }
                            else
                            {
                                Editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                                randAccStream.Dispose();
                            }


                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox =
                                new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        saved = true;
                        AppTitle.Text = file.Name + " - " + appTitleStr;
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                    }
                }
                catch (Exception)
                {
                    SaveFile(true);
                }

            }
        }

        private async void SaveAsDOCX_Click(object sender, RoutedEventArgs e)
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            if (isCopy || fileName == "Document")
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Office Open XML Document", new List<string>() { ".docx" });
                savePicker.FileTypeChoices.Add("Formatted Document", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Text Document", new List<string>() { ".txt" });
                savePicker.FileTypeChoices.Add("OpenDocument Text", new List<string>() { ".odt" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";


                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                        switch (file.Name.EndsWith(".txt"))
                        {
                            case false:
                                // RTF file, format for it
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                            case true:
                                // TXT File, disable RTF formatting so that this is plain text
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                        }


                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox =
                            new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    AppTitle.Text = file.Name + " - " + appTitleStr;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Document")
            {
                string path = fileNameWithPath.Replace("\\" + fileName, "");
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            if (file.Name.EndsWith(".txt"))
                            {
                                Editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                                randAccStream.Dispose();
                            }
                            else
                            {
                                Editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                                randAccStream.Dispose();
                            }


                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox =
                                new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        saved = true;
                        AppTitle.Text = file.Name + " - " + appTitleStr;
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                    }
                }
                catch (Exception)
                {
                    SaveFile(true);
                }

            }
        }

        private async void SaveAsODT_Click(object sender, RoutedEventArgs e)
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            if (isCopy || fileName == "Document")
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("OpenDocument Text", new List<string>() { ".odt" });
                savePicker.FileTypeChoices.Add("Formatted Document", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Text Document", new List<string>() { ".txt" });
                savePicker.FileTypeChoices.Add("Office Open XML Document", new List<string>() { ".docx" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";


                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                        switch (file.Name.EndsWith(".txt"))
                        {
                            case false:
                                // RTF file, format for it
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                            case true:
                                // TXT File, disable RTF formatting so that this is plain text
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                        }


                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox =
                            new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    AppTitle.Text = file.Name + " - " + appTitleStr;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Document")
            {
                string path = fileNameWithPath.Replace("\\" + fileName, "");
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            if (file.Name.EndsWith(".txt"))
                            {
                                Editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                                randAccStream.Dispose();
                            }
                            else
                            {
                                Editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                                randAccStream.Dispose();
                            }


                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox =
                                new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        saved = true;
                        AppTitle.Text = file.Name + " - " + appTitleStr;
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                    }
                }
                catch (Exception)
                {
                    SaveFile(true);
                }

            }
        }

        private async void SaveAsTXT_Click(object sender, RoutedEventArgs e)
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            if (isCopy || fileName == "Document")
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Text Document", new List<string>() { ".txt" });
                savePicker.FileTypeChoices.Add("Formatted Document", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("OpenDocument Text", new List<string>() { ".odt" });
                savePicker.FileTypeChoices.Add("Office Open XML Document", new List<string>() { ".docx" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";


                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                        switch (file.Name.EndsWith(".txt"))
                        {
                            case false:
                                // RTF file, format for it
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                            case true:
                                // TXT File, disable RTF formatting so that this is plain text
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                        }


                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox =
                            new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    AppTitle.Text = file.Name + " - " + appTitleStr;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Document")
            {
                string path = fileNameWithPath.Replace("\\" + fileName, "");
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            if (file.Name.EndsWith(".txt"))
                            {
                                Editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                                randAccStream.Dispose();
                            }
                            else
                            {
                                Editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                                randAccStream.Dispose();
                            }


                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox =
                                new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        saved = true;
                        AppTitle.Text = file.Name + " - " + appTitleStr;
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                    }
                }
                catch (Exception)
                {
                    SaveFile(true);
                }

            }
        }

        private async void SaveAsOther_Click(object sender, RoutedEventArgs e)
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            if (isCopy || fileName == "Document")
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Dropdown of file types the user can save the file as

                savePicker.FileTypeChoices.Add("Formatted Document", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Text Document", new List<string>() { ".txt" });
                savePicker.FileTypeChoices.Add("OpenDocument Text", new List<string>() { ".odt" });
                savePicker.FileTypeChoices.Add("Office Open XML Document", new List<string>() { ".docx" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";


                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                        switch (file.Name.EndsWith(".txt"))
                        {
                            case false:
                                // RTF file, format for it
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                            case true:
                                // TXT File, disable RTF formatting so that this is plain text
                                {
                                    Editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, randAccStream);
                                    randAccStream.Dispose();
                                }
                                break;
                        }


                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox =
                            new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    AppTitle.Text = file.Name + " - " + appTitleStr;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Document")
            {
                string path = fileNameWithPath.Replace("\\" + fileName, "");
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            if (file.Name.EndsWith(".txt"))
                            {
                                Editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                                randAccStream.Dispose();
                            }
                            else
                            {
                                Editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                                randAccStream.Dispose();
                            }


                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox =
                                new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        saved = true;
                        AppTitle.Text = file.Name + " - " + appTitleStr;
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                    }
                }
                catch (Exception)
                {
                    SaveFile(true);
                }

            }



        }

        private async void NewDoc_Click(object sender, RoutedEventArgs e)
        {
            await ShowUnsavedDialogSE();

        }


        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.Subject = "Hello";
            string value = string.Empty;
            Editor.Document.GetText(TextGetOptions.None, out value);
            emailMessage.Body = value;
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private void AlignJustifyButton_Click(object sender, RoutedEventArgs e)
        {
            Editor.AlignSelectedTo(RichEditHelpers.AlignMode.Justify);
            editor_SelectionChanged(sender, e);
        }

        private void DecreaseZoomButton_Click(object sender, RoutedEventArgs e)
        {
            // Decrease the zoom by 10%, but don't go below the minimum value of the slider
            ZoomSlider.Value = Math.Max(ZoomSlider.Value - 0.1, ZoomSlider.Minimum);
        }

        private void IncreaseZoomButton_Click(object sender, RoutedEventArgs e)
        {
            // Increase the zoom by 10%, but don't exceed the maximum value of the slider
            ZoomSlider.Value = Math.Min(ZoomSlider.Value + 0.1, ZoomSlider.Maximum);
        }

        private void AnimateZoomSecond(double fromValue, double toValue)
        {
            // Get the scale transform of the rich edit box
            var scaleTransform = EditorGrid.RenderTransform as ScaleTransform;

            // Create a storyboard to animate the scale x and y properties
            var storyboard = new Storyboard();
            var animationX = new DoubleAnimation()
            {
                From = fromValue,
                To = toValue,
                Duration = TimeSpan.FromSeconds(0.2), // Adjust the duration as needed
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut } // Adjust the easing function as needed
            };
            var animationY = new DoubleAnimation()
            {
                From = fromValue,
                To = toValue,
                Duration = TimeSpan.FromSeconds(0.2), // Adjust the duration as needed
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut } // Adjust the easing function as needed
            };
            storyboard.Children.Add(animationX);
            storyboard.Children.Add(animationY);

            // Set the target of the animation to be the scale x and y properties of the scale transform
            Storyboard.SetTarget(animationX, scaleTransform);
            Storyboard.SetTargetProperty(animationX, "ScaleX");
            Storyboard.SetTarget(animationY, scaleTransform);
            Storyboard.SetTargetProperty(animationY, "ScaleY");

            // Start the animation
            storyboard.Begin();
        }

        private void MenuCut_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.Cut();
        }

        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.Copy();
        }

        private void MenuPaste_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Selection.Paste(0);
        }

        private async void MenuParagraph_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the ParagraphDialog
            ParagraphDialog paragraphDialog = new ParagraphDialog();

            // Show the dialog and wait for the user's input
            ContentDialogResult result = await paragraphDialog.ShowAsync();

            // If the user clicked the OK button, adjust the properties of the RichEditBox
            if (result == ContentDialogResult.Primary)
            {
                // Get the values from the dialog's TextBoxes and ComboBoxes
                TextBox leftTextBox = (TextBox)paragraphDialog.FindName("LeftTextBox");
                TextBox rightTextBox = (TextBox)paragraphDialog.FindName("RightTextBox");
                TextBox firstLineTextBox = (TextBox)paragraphDialog.FindName("FirstLineTextBox");
                ComboBox lineSpacingComboBox = (ComboBox)paragraphDialog.FindName("LineSpacingComboBox");

                // Parse the values and set the properties of the RichEditBox
                double left = double.Parse(leftTextBox.Text);
                double right = double.Parse(rightTextBox.Text);
                double firstLine = double.Parse(firstLineTextBox.Text);
                double lineSpacing = double.Parse(lineSpacingComboBox.SelectedItem.ToString());

                Editor.Margin = new Thickness(left, 0, right, 0);
                Editor.Document.Selection.ParagraphFormat.SetIndents((float)firstLine, 0, 0);
                Editor.Document.Selection.ParagraphFormat.SetLineSpacing(Windows.UI.Text.LineSpacingRule.AtLeast, (float)lineSpacing);
            }
        }

        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void PageSetup_Click(object sender, RoutedEventArgs e)
        {
            openpageprop();
        }

        private double ConvertToUnit(double value, string unit)
        {
            switch (unit)
            {
                case "Inches":
                    return value;
                case "Centimeters":
                    return value * 2.54; // Convert inches to centimeters
                case "Points":
                    return value * 72; // Convert inches to points
                case "Picas":
                    return value * 6; // Convert inches to picas
                default:
                    return value; // Default to inches
            }
        }

        private async void openpageprop()
        {
            // Create an instance of the ParagraphDialog
            Pageprop pageprop = new Pageprop();

            // Show the dialog and wait for the user's input
            ContentDialogResult result = await pageprop.ShowAsync();

            // If the user clicked the OK button, adjust the properties of the RichEditBox
            if (result == ContentDialogResult.Primary)
            {

                // Get the values from the dialog's TextBoxes and ComboBoxes
                TextBox LeftMarginTextBox = (TextBox)pageprop.FindName("LeftMarginTextBox");
                TextBox RightMarginTextBox = (TextBox)pageprop.FindName("RightMarginTextBox");
                TextBox TopMarginTextBox = (TextBox)pageprop.FindName("TopMarginTextBox");
                TextBox BottomMarginTextBox = (TextBox)pageprop.FindName("BottomMarginTextBox");

                TextBlock marginsname = (TextBlock)pageprop.FindName("marginsname");

                ComboBox PaperTypeCombo = (ComboBox)pageprop.FindName("PaperTypeCombo");
                RadioButton orientationportait = (RadioButton)pageprop.FindName("orientationportait");
                CheckBox printpagenumbers = (CheckBox)pageprop.FindName("printpagenumbers");

                // Save the selected paper size and orientation
                var settings = ApplicationData.Current.LocalSettings;
                if (PaperTypeCombo.SelectedItem != null)
                {
                    string selectedPaperSize = (PaperTypeCombo.SelectedItem as ComboBoxItem).Content.ToString();
                    settings.Values["papersize"] = selectedPaperSize;
                }

                settings.Values["orientation"] = orientationportait.IsChecked == true ? "Portrait" : "Landscape";

                // Save margin values
                settings.Values["pagesetupLmargin"] = ConvertToUnit(double.Parse(LeftMarginTextBox.Text), marginsname.Text);
                settings.Values["pagesetupRmargin"] = ConvertToUnit(double.Parse(RightMarginTextBox.Text), marginsname.Text);
                settings.Values["pagesetupTmargin"] = ConvertToUnit(double.Parse(TopMarginTextBox.Text), marginsname.Text);
                settings.Values["pagesetupBmargin"] = ConvertToUnit(double.Parse(BottomMarginTextBox.Text), marginsname.Text);

                // Save Print Page Numbers setting
                settings.Values["isprintpagenumbers"] = printpagenumbers.IsChecked == true ? "yes" : "no";

                Dictionary<string, (double Width, double Height)> paperSizes = pageprop.paperSizes;

                string selectedPaperSizea = (PaperTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedPaperSizea) && paperSizes.TryGetValue(selectedPaperSizea, out var dimensions))
                {
                    double originalWidth = 812; // Original RichEditBox width
                    double originalHeight = 1116; // Original RichEditBox height

                    double width = dimensions.Width;
                    double height = dimensions.Height;

                    // Calculate the scaling factors for width and height to maintain the aspect ratio
                    double widthScaleFactor = width / originalWidth;
                    double heightScaleFactor = height / originalHeight;

                    // Determine the scaling factor that fits the width within the original width
                    double widthFitScaleFactor = originalWidth / width;

                    // Determine the scaling factor that fits the height within the original height
                    double heightFitScaleFactor = originalHeight / height;

                    // Choose the minimum scaling factor to ensure the content fits entirely within the original dimensions
                    double minScaleFactor = Math.Min(widthFitScaleFactor, heightFitScaleFactor);

                    // Apply the minimum scaling factor to both width and height to maintain the aspect ratio
                    width *= minScaleFactor;
                    height *= minScaleFactor;

                    // Set the UWP's RichEditBox width and height
                    EditorGrid.Width = width;
                    EditorGrid.Height = height;
                }
            }
        }

        private void PageSetupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            openpageprop();
        }
    }
}
