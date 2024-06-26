# WPF Radiance Temperature Application

## Overview
This application calculates a Radiance Temperature relationship, using filters and input data (wavelength range, temperature limits, resolution). The Sakuma Hattori Planck III approximation calculates expected signal for a Temperature and Wavelength, and gives a good result for narrow band applications.

## Features
- **Filter Management**: Add, delete, and stack filters dynamically.
- **Temperature-Radiance Calculation**: Generate relationship for specified temperature range and steps, and wavelength band.
- **Data Preview/Output**: Preview filter data and Temperature-Radiance result graph, download Temperature-Radiance values quantised to temperature step as `.csv`.

## Getting Started
To run this application locally, follow these steps:

### Prerequisites
- .NET 8 Framework installed

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/willhampshire/WPFapp_radiance_temperature.git
   ```
   
2. Open the project in IDE.

### Usage
1. Compile the project in Release mode.
2. Run the application.
3. Use the application to create Temperature-Radiance relationship.

## Contributing
Contributions are welcome! Please fork the repository and create a pull request with your improvements.

## License
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Acknowledgments
- Built using C# and .NET Framework usinf Rider IDE student license.
- Utilizes SQLite for database operations.
