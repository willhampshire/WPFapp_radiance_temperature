using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using WpfApp1.DBtools;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace WpfApp1
{
    using TempRawList = List<Tuple<double, double>>;
    public class FilterUpload
    {
        public double Wavelength { get; set; }
        public double Transmission { get; set; }
    }

    
    public partial class MainWindow : Window
    {
        private ObservableCollection<FilterUpload> filter;
        
        public PlotModel PlotModel { get; set; }

        public string activeFilter = null;
        public string activeSelectedFilter = null; //global variable for active selected filter
        public string selectFilter = null;
        public HashSet<string> setFilters = new HashSet<string>();
        
        public MainWindow()
        {
            InitializeComponent();
            List<string> existingFilters = Database.InitialiseDatabase();
            foreach (var filterName in existingFilters)
            {
                FiltersDatabaseListBox.Items.Add(filterName);
                setFilters.Add(filterName);
            }
            FiltersDatabaseListBox.SelectionChanged += FiltersDatabaseListBox_SelectionChanged;
            ActiveFiltersListBox.SelectionChanged += ActiveFiltersListBox_SelectionChanged;

        }
        
        public class FilterPreview
        {
            public double Wavelength { get; set; }
            public double Transmission { get; set; }

            public FilterPreview(double wavelength, double transmission)
            {
                Wavelength = wavelength;
                Transmission = transmission;
            }
        }
        private ObservableCollection<FilterPreview> filterPreviewData;
        public double[] inputParameters;
        public string validateExternalErrorMessage;
        public double[] radianceArray;
        public double[] temperatureArray;
        public bool scaleshapeStatus;
        private bool validateInputParameters()
        {
            ValidateInfoText.Text = "";
            //Get the values from the text fields and convert to double
            string[] parametersNames = { "LowlimText", "HighLimText", "LowTempText", "HighTempText", "StepText" };
            string[] parametersValuesStr = new string[parametersNames.Length];
            double[] parametersValuesDouble = new double[parametersNames.Length];
                    
            for (int i = 0; i < parametersNames.Length; i++)
            {
                TextBox textBox = FindName(parametersNames[i]) as TextBox;
                if (textBox != null)
                {
                    parametersValuesStr[i] = textBox.Text;
                    Console.WriteLine($"{parametersNames[i]}: {textBox.Text}");
                }
            }
            bool writtenMessageDoubles = false;        
            for (int i = 0; i < parametersNames.Length; i++)
            {

                if (double.TryParse(parametersValuesStr[i], out double number))
                {
                    
                    if (number != 0)
                    {
                        // Conversion successful
                        parametersValuesDouble[i] = number;
                        Console.WriteLine($"Converted to double: {number}");
                        if (!writtenMessageDoubles)
                        {
                            ValidateInfoText.Text += "Converted inputs to doubles successfully.";
                            writtenMessageDoubles = true;
                        }
                        
                    }
                    else
                    {
                        if (i != 2) //allow bottom temperature to be 0
                        {
                            ValidateInfoText.Text = $"Error - Input '{number}' is 0";
                            return false;
                        }
                        
                    }
                }
                else
                {
                    // Conversion failed
                    Console.WriteLine($"Unable to convert input {parametersValuesStr[i]} to double.");
                    ValidateInfoText.Text = "Error - Unable to convert one/many of inputs to double.";
                    return false;
                }
            }

            inputParameters = parametersValuesDouble;

            bool radianceParamsCheck = checkRadiance();
            bool interpParamsCheck = checkInterpolation();
            if (!interpParamsCheck || !radianceParamsCheck)
            {
                ValidateInfoText.Text = $"Interpolation parameters error: {validateExternalErrorMessage}";
                return false;
            }
            
            return true;
        }

        private bool checkRadiance()
        {
            double waveLow = inputParameters[0];
            double waveHigh = inputParameters[1];

            if (waveLow >= waveHigh)
            {
                validateExternalErrorMessage = $"E{waveLow} >= {waveHigh}";
                return false;
            }

            return true;
        }
        private bool checkInterpolation()
        {
            double tempStart = inputParameters[2];
            double tempStop = inputParameters[3];
            double tempStep = inputParameters[4]; //kept all as double for calculation type-safety

            if (tempStep == Math.Floor(tempStep))
            {
                int tempStepInt = Convert.ToInt16(tempStep);
            }
            else
            {
                validateExternalErrorMessage = $"Could not convert {tempStep} to Int16";
                return false;
            }

            double stepRemainder = (tempStop - tempStart) % tempStep;

            if (tempStop - tempStart < tempStep)
            {
                validateExternalErrorMessage = $"Invalid start/stop/step: tempStop - tempStart < tempStep";
                return false;
            }

            if (tempStart > tempStop)
            {
                validateExternalErrorMessage = "Invalid start/stop/step: tempStart > tempStep";
            }

            if (stepRemainder == 0)
            {
                ValidateInfoText.Text += "\nStep is correct for start and stop temperatures.";
            }
            else
            {
                validateExternalErrorMessage = $"Invalid start/stop/step - remainder {stepRemainder}";
                return false;
            }
            return true;
        }

        private bool sendToInputTab = false;
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("SELECTION");
            // Check which tab item is selected
            TabItem selectedTab = null;
            try
            {
                selectedTab = e.AddedItems[0] as TabItem;
            }
            catch
            {
                Console.WriteLine("No tab changed");
            }

            if (selectedTab != null && !sendToInputTab)
            {
                if (selectedTab.Header.ToString() == "Output")
                {
                    bool checkInput = validateInputParameters();

                    if (checkInput)
                    {
                        List<string> allActiveFilters = new List<string>();
                        
                        foreach (var item in ActiveFiltersListBox.Items)
                        {
                            allActiveFilters.Add(item.ToString());
                        }
                        
                        scaleshapeStatus = Calculations.CalculateScaleShape(inputParameters, allActiveFilters, out radianceArray, out temperatureArray);
                        
                        bool saveData = SaveData(temperatureArray, radianceArray);
                        if (!saveData)
                        {
                            MessageBox.Show("Error - save data");
                        }
                        
                        LineSeries lineSeries = new LineSeries
                        {
                            MarkerType = MarkerType.Cross
                        };
                        
                        for (int t = 0; t < temperatureArray.Length; t++)
                        {
                            lineSeries.Points.Add(new DataPoint(temperatureArray[t], radianceArray[t]));
                        }
                        
                        PlotModel = new PlotModel { Title = "Radiance against Temperature" };
                        LinearAxis xAxis = new LinearAxis
                        {
                            Position = AxisPosition.Bottom,
                            Title = "Temperature [C]" // X-axis label
                        };
                        LinearAxis yAxis = new LinearAxis
                        {
                            Position = AxisPosition.Left,
                            Title = "Radiance [arb]" // Y-axis label
                        };
                        
                        PlotModel.Axes.Add(xAxis);
                        PlotModel.Axes.Add(yAxis);
                        PlotModel.Series.Add(lineSeries);
                        
                        RadianceTemperatureOxyPlot.Model = PlotModel; // Assign the PlotModel to the PlotView
                        RadianceTemperatureOxyPlot.InvalidatePlot(true); // Refresh the plot
                    }
                    else
                    {
                        MessageBox.Show("Input data contains invalid data.\nPlease see VALIDATION text.");
                        MainTabControl.SelectedIndex = 0;
                        sendToInputTab = true;
                    }
                }
            } else if (sendToInputTab)
            {
                MainTabControl.SelectedIndex = 0; // needs setting again as output tab still selected as message box open
                sendToInputTab = false;
            }
        }
        
        
        private void FiltersDatabaseListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if there is a selected item
            if (FiltersDatabaseListBox.SelectedItem != null)
            {
                string selectedFilter = FiltersDatabaseListBox.SelectedItem as string;

                if (!string.IsNullOrEmpty(selectedFilter))
                {
                    // Perform actions based on the selected filter
                    Console.WriteLine($"Selected filter: {selectedFilter}");
                    selectFilter = selectedFilter;

                    List<(double Wavelength, double Transmission)> filterData = Database.GetFilterData(selectedFilter);
                    
                    filterPreviewData = new ObservableCollection<FilterPreview>();

                    foreach (var data in filterData)
                    {
                        filterPreviewData.Add(new FilterPreview(data.Wavelength, data.Transmission));
                    }

                    // Bind the retrieved data to the PreviewDataGrid
                    PreviewDataGrid.ItemsSource = filterPreviewData;
                    
                }
                else
                {
                    selectFilter = "";
                }
            }
        }

        
        private void ActiveFiltersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveFiltersListBox.SelectedItem != null)
            {
                string acSelectedFilter = ActiveFiltersListBox.SelectedItem as string;
                if (!string.IsNullOrEmpty(acSelectedFilter))
                {
                    activeSelectedFilter = acSelectedFilter;
                }
                else
                {
                    activeSelectedFilter = "";
                }
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectFilter != "")
            {
                activeFilter = selectFilter;
                Console.WriteLine($"Active filter {activeFilter}");
                //ActiveFilterText.Text = activeFilter;
                if (!ActiveFiltersListBox.Items.Contains(activeFilter))
                {
                    ActiveFiltersListBox.Items.Add(activeFilter);
                }
                else
                {
                    MessageBox.Show("Filter already in active filters");
                }
            }
            else
            {
                MessageBox.Show("Please select valid filter");
            }
            
        }

        private void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveFiltersListBox.Items.Contains(activeSelectedFilter))
            {
                ActiveFiltersListBox.Items.Remove(activeSelectedFilter);
            }
            else
            {
                MessageBox.Show("Filter does not exist in active filters.");
            }
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Validating parameters");
            validateInputParameters();
        }


        private void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            string name;
            string newName;
            List<(double wavelength, double transmission)> filterData = new List<(double wavelength, double transmission)>();
            
            try
            {
                name = FiltersDatabaseListBox.SelectedItem.ToString();
                newName = name + "_copy";
            }
            catch
            {
                name = null;
                newName = null;
            }

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(newName) && name != "Filters")
            {
                if (!setFilters.Contains(newName) && !FiltersDatabaseListBox.Items.Contains(newName))
                {
                    //GetFilterData AddFilter
                    filterData = Database.GetFilterData(name);
                    Database.AddFilter(newName, filterData);
                    Console.WriteLine($"Added {newName} to database");
                    FiltersDatabaseListBox.Items.Add(newName);
                }
                else
                {
                    MessageBox.Show($"'{newName}' already exists");
                }
            }
            else
            {
                MessageBox.Show($"Either '{name}' or '{newName}' invalid");
            }
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            string oldName;
            try
            {
                oldName = FiltersDatabaseListBox.SelectedItem.ToString();
            }
            catch
            {
                MessageBox.Show("No filter selected to rename");
                oldName = null;
                return;
            }
            
            string newName = NewFilterName.Text.Replace(" ", "_");
            if (!string.IsNullOrEmpty(newName) && newName != "Filters" && !setFilters.Contains(newName) && !FiltersDatabaseListBox.Items.Contains(newName))
            {
                Database.RenameFilter(oldName, newName);
            
                // remove old name from filter db listbox, add new
                FiltersDatabaseListBox.Items.Remove(oldName);
                FiltersDatabaseListBox.Items.Add(newName);
                FiltersDatabaseListBox.SelectedItem = newName;

                if (ActiveFiltersListBox.Items.Contains(oldName))
                {
                    // same in active filters listbox
                    ActiveFiltersListBox.Items.Remove(oldName);
                    ActiveFiltersListBox.Items.Add(newName);
                }
                
            }
            else
            {
                MessageBox.Show($"Name '{newName}' invalid");
            }
        }
        
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            
            string name = NewFilterName.Text.Replace(" ", "_"); // remove spaces - table names should not contain spaces
            
            if (filter != null && filter.Count > 0)
            {
                if (!string.IsNullOrEmpty(name) && name != "Filters" && !setFilters.Contains(name))
                {
                    setFilters.Add(name);
                    Console.WriteLine($"{setFilters} does not contain {name}");
                    // Prepare a list to hold wavelength and transmission data
                    List<(double wavelength, double transmission)> data = new List<(double wavelength, double transmission)>();

                    foreach (var item in filter)
                    {
                        data.Add((item.Wavelength, item.Transmission));
                    }

                    // Add filter to database
                    Database.AddFilter(name, data);
                    Console.WriteLine($"Added {name} to database");
                    FiltersDatabaseListBox.Items.Add(name);
                    //FiltersDatabaseListBox.Items.Refresh();
                }
                else
                {
                    MessageBox.Show($"Name '{name}' is invalid.");
                }
                
                
            }
            else
            {
                MessageBox.Show($"No filter data uploaded.");
            }
            
        }
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string name;
            try
            {
                name = FiltersDatabaseListBox.SelectedItem.ToString();
            }
            catch
            {
                name = null;
            }
            

            if (name != null && name != "")
            {
                Database.DeleteFilter(name);
                Console.WriteLine($"Dropped {name} from database");
                FiltersDatabaseListBox.Items.Remove(name);
                FiltersDatabaseListBox.SelectedIndex = -1;
                ActiveFiltersListBox.Items.Remove(name);
                ActiveFiltersListBox.SelectedIndex = -1;
                setFilters.Remove(name);
                return;
            }
            
            MessageBox.Show($"Could not delete filter '{name}'", "Delete filter from DB Error");
            selectFilter = "";
            
        }
    
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    filter = new ObservableCollection<FilterUpload>();

                    using (var reader = new StreamReader(openFileDialog.FileName))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();

                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            string[] values = line.Split(',');

                            if (values.Length != 2)
                            {
                                MessageBox.Show($"Skipping invalid line: {line}");
                                continue;
                            }

                            if (double.TryParse(values[0], out double wavelength) && double.TryParse(values[1], out double transmission))
                            {
                                
                                var newFilter = new FilterUpload()
                                {
                                    Wavelength = wavelength,
                                    Transmission = transmission
                                };
                                filter.Add(newFilter);
                            }
                            else
                            {
                                MessageBox.Show($"Skipping line with invalid data: {line}");
                            }
                        }
                    }

                    // Bind ObservableCollection to DataGrid
                    PreviewDataGrid.ItemsSource = filter;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading CSV file: {ex.Message}");
                }
            }
        }

        private void UploadFilterInfo(object sender, RoutedEventArgs routedEventArgs)
        {
            string filterInfoMsg = "Wavelength units - micron \nMust not have headers in CSV, first row should be wavelength-transmission pair.";
            MessageBox.Show(filterInfoMsg, "Filter CSV Info");
        }
        

        public TempRawList results;
        private bool SaveData(double[] temperatureArray, double[] radianceArray)
        {
            double tempStart = inputParameters[2];
            double tempStop = inputParameters[3];
            double tempStep = inputParameters[4];

            double radiance;
            TempRawList radianceTemperature = new TempRawList();

            for (double temp = tempStart; temp <= tempStop; temp += tempStep)
            {
                Console.WriteLine($"Temp: {temp} step: {tempStep}");
                //interp even though not required in case rounding errors occur meaning exact double value cannot be
                //found, or in future to change output vs input resolution.
                bool interpTempRadiance = Calculations.InterpTemperatureRadiance(temperatureArray, radianceArray, temp, out radiance);
                radianceTemperature.Add(new Tuple<double, double>(temp, radiance));
                if (!interpTempRadiance)
                {
                    return false;
                }
            }
            Console.WriteLine("Temperature Radiance data created");
            results = radianceTemperature;
            return true;
        }

        private void DownloadResults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.Title = "Save CSV File";
                bool? result = saveFileDialog.ShowDialog();

                // If the user clicked OK, proceed with file creation
                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // Create or overwrite the file
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        // Write the header
                        writer.WriteLine("Temperature,Radiance");

                        // Write the data
                        foreach (var dataPoint in results)
                        {
                            writer.WriteLine($"{dataPoint.Item1},{dataPoint.Item2}");
                        }
                    }

                    Console.WriteLine("File successfully created at " + filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to file: " + ex.Message);
            }
        }
        
    }
}
