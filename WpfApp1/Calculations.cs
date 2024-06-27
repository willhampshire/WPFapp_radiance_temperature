using System.Windows;
using WpfApp1.DBtools;

namespace WpfApp1
{
    using ListOfFilterDataLists = List<List<(double Wavelength, double Transmission)>>;
    using FilterDataList = List<(double Wavelength, double Transmission)>;
    public class SHCalculations
    {
        private const double C1Prime = 3.74177e-16; // W*m^2
        private const double C2 = 1.4388e-2; // m*K
        private const double C3 = 0; // Empirical constant, adjust as needed

        // Function to calculate spectral radiance using Sakuma-Hattori Planck 3 approximation
        public static bool SakumaHattoriThree(double[] wavelengths, double[] temperatures,
            List<(double Wavelength, double Transmission)> filterData, out double[] radianceValues)
        {
            radianceValues = null;

            if (wavelengths.Length != 2)
            {
                throw new ArgumentException("Wavelengths array must contain exactly 2 values: low and high.");
            }

            // Initialize radianceValues array
            radianceValues = new double[temperatures.Length]; // 1D array for total radiance

            double wavelengthLow = wavelengths[0] / 1e6; // Convert microns to meters
            double wavelengthHigh = wavelengths[1] / 1e6;
            int numIntegrationPoints = 500;
            double deltaWavelength = (wavelengthHigh - wavelengthLow) / (numIntegrationPoints - 1);

            // Constants (adjust units if necessary)
            const double C1Prime = 3.74177e-16; // W*m^2
            const double C2 = 1.4388e-2; // m*K
            const double C3 = 0; // Empirical constant, adjust as needed

            // Calculate total radiance (emissivity) for each temperature
            for (int t = 0; t < temperatures.Length; t++)
            {
                double temperatureCelsius = temperatures[t]; // Temperature in Celsius
                double temperatureKelvin = temperatureCelsius + 273.15; // Convert Celsius to Kelvin

                double totalRadiance = 0.0;

                // Integrate radiance over the wavelength range using trapezoidal rule
                for (int i = 0; i < numIntegrationPoints - 1; i++)
                {
                    double wavelength1 = wavelengthLow + i * deltaWavelength;
                    double wavelength2 = wavelengthLow + (i + 1) * deltaWavelength;

                    // Compute spectral radiance using SHP approximation
                    double radiance1 = C1Prime / (Math.Pow(wavelength1, 5) *
                                                  (Math.Exp(C2 / (wavelength1 * (temperatureKelvin + C3))) - 1));
                    double radiance2 = C1Prime / (Math.Pow(wavelength2, 5) *
                                                  (Math.Exp(C2 / (wavelength2 * (temperatureKelvin + C3))) - 1));

                    // Apply filter transmission attenuation if applicable
                    double filterTransmission1 = 1.0; // Default to no attenuation
                    double filterTransmission2 = 1.0; // Default to no attenuation

                    if (filterData != null && filterData.Count > 0)
                    {
                        filterTransmission1 =
                            Calculations.InterpFilterTransmission(wavelength1 * 1e6,
                                filterData); // Convert back to microns for interpolation
                        filterTransmission2 =
                            Calculations.InterpFilterTransmission(wavelength2 * 1e6,
                                filterData); // Convert back to microns for interpolation
                    }

                    radiance1 *= filterTransmission1;
                    radiance2 *= filterTransmission2;

                    // Accumulate total radiance (emissivity) using trapezoidal rule
                    totalRadiance += 0.5 * (radiance1 + radiance2) * deltaWavelength;
                }

                // Store the total radiance in the output array
                radianceValues[t] = totalRadiance;
            }

            return true; // Calculation successful
        }
    }


    public class Calculations
    {
        public static bool InterpTemperatureRadiance(double[] temperatureArray, double[] radianceArray, double temp, out double rad)
        {
            
            // Check if the temperature is outside the bounds of the array
            if (temp <= temperatureArray[0])
            {
                rad = radianceArray[0];
                //MessageBox.Show("Error - temperature outside of range");
                return true;
            }
            
            if (temp >= temperatureArray[temperatureArray.Length - 1])
            {
                rad = radianceArray[radianceArray.Length - 1];
                //MessageBox.Show("Error - temperature outside of range");
                return true;
            }

            // Find the surrounding temperatures for interpolation
            for (int i = 0; i < temperatureArray.Length - 1; i++)
            {
                if (temp >= temperatureArray[i] && temp <= temperatureArray[i + 1])
                {
                    // Perform linear interpolation
                    double t1 = temperatureArray[i];
                    double t2 = temperatureArray[i + 1];
                    double r1 = radianceArray[i];
                    double r2 = radianceArray[i + 1];

                    // Calculate the interpolated radiance
                    rad = r1 + ((temp - t1) / (t2 - t1)) * (r2 - r1);
                    return true;
                }
            }

            rad = 0;
            return false;
        }
        
        public static double InterpFilterTransmission(double wavelengthMicrons, List<(double Wavelength, double Transmission)> filterData)
        {
            // Find the two closest wavelengths in filterData
            var orderedData = filterData.OrderBy(fd => Math.Abs(fd.Wavelength - wavelengthMicrons)).ToList();

            // Edge case: if wavelengthMicrons is outside the range of filterData, return transmission of closest endpoint
            if (orderedData.Count == 0)
                throw new ArgumentException("Filter data is empty.");

            if (wavelengthMicrons < orderedData[0].Wavelength)
                return orderedData[0].Transmission;

            if (wavelengthMicrons > orderedData[^1].Wavelength)
                return orderedData[^1].Transmission;

            // Find the two closest wavelengths
            int index = orderedData.FindIndex(fd => fd.Wavelength > wavelengthMicrons);
            var (wavelength1, transmission1) = orderedData[index - 1];
            var (wavelength2, transmission2) = orderedData[index];

            // Perform linear interpolation
            double interpolatedTransmission = transmission1 + (transmission2 - transmission1) * (wavelengthMicrons - wavelength1) / (wavelength2 - wavelength1);
    
            return interpolatedTransmission;
        }
        
        public static double[] Linspace(double start, double stop, double step)
        {
            List<double> result = new List<double>();

            double value = start;

            if (step > 0)
            {
                while (value <= stop)
                {
                    result.Add(value);
                    value += step;
                }
            }
            else
            {
                while (value >= stop)
                {
                    result.Add(value);
                    value += step;
                }
            }

            return result.ToArray();
        }

        public static bool CalculateScaleShape(double[] parameters, List<string> filterNames, out double[] radianceArray, out double[] temperatures)
        {
            double wL = parameters[0];
            double wH = parameters[1];
            double tL = parameters[2];
            double tH = parameters[3];
            double step = parameters[4];

            double[] wavelengths = new double[] { wL, wH };
            
            ListOfFilterDataLists allFilterData = new ListOfFilterDataLists();
            FilterDataList combinedFilterData = null;

            Console.WriteLine("Calculating scale shape...");
            try
            {
                temperatures = Linspace(tL, tH, step);
                string filterName;

                if (filterNames.Count > 0)
                {
                    for (int i = 0; i < filterNames.Count; i++)
                    {
                        filterName = filterNames[i];
                        List<(double Wavelength, double Transmission)> filterData;
                        try
                        {
                            filterData = Database.GetFilterData(filterName);
                            allFilterData.Add(filterData);
                        }
                        catch
                        {
                            filterData = null;
                            Console.WriteLine($"Filter {filterName} database data not found.");
                        }
                    }

                    bool boolFiltersCombined = CombineFilters(allFilterData, out combinedFilterData);
                    if (!boolFiltersCombined)
                    {
                        MessageBox.Show("Error combining filters");
                        radianceArray = null;
                        return false;
                    }
                }
                else
                {
                    combinedFilterData = null;
                }
                
                
                SHCalculations.SakumaHattoriThree(wavelengths, temperatures, combinedFilterData, out radianceArray);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                radianceArray = null;
                temperatures = null;
                return false;
            }
        }

        public static bool CombineFilters(ListOfFilterDataLists allFilters, out FilterDataList combinedFilter)
        {
            FilterDataList baseFilter; // base filter - overwritten with result filter iteratively when merged with-
            FilterDataList newFilter; // merged with base filter
            
            //MessageBox.Show($"{allFilters.Count} filters"); debug
            if (allFilters.Count == 1)
            {
                combinedFilter = allFilters[0];
                Console.WriteLine("1 filter present");
            }
            else if (allFilters.Count > 1)
            {
                baseFilter = allFilters[0]; // init set to first filter
                for (int i = 1; i < allFilters.Count; i++) // iterate n-1 times for n filters
                {
                    newFilter = allFilters[i]; // Get the new filter to be merged

                    // Create a new list to store combined filter data
                    FilterDataList combinedTemp = new FilterDataList();

                    // Iterate through baseFilter and interpolate with newFilter
                    foreach (var (wavelength, transmissionBase) in baseFilter)
                    {
                        double interpolatedTransmission = InterpFilterTransmission(wavelength, newFilter);
                        combinedTemp.Add((wavelength, transmissionBase * interpolatedTransmission));
                    }

                    // Iterate through newFilter and interpolate with baseFilter
                    foreach (var (wavelength, transmissionNew) in newFilter)
                    {
                        double interpolatedTransmission = InterpFilterTransmission(wavelength, baseFilter);
                        combinedTemp.Add((wavelength, transmissionNew * interpolatedTransmission));
                    }

                    baseFilter = combinedTemp;
                }

                combinedFilter = baseFilter;
            }
            else
            {
                combinedFilter = null;
                
                return false;
            }

            return true;
        }
    }
}
