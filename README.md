# HereRoadInspector

The HereRoadInspector is a console application that provides functionalities related to road inspections using the HERE Maps API. It offers the following features:

#### **Check if a Location is on a Street:** Given a location, the application determines if the location is on a street or not.

#### **Validate Route Direction:** The application checks if a given route is in a valid direction.

#### **Check Speed Limit:** For a given location, the application retrieves the speed limit.
Dependencies

### **The project relies on several NuGet packages, including:**

NetTopologySuite
Newtonsoft.Json
System.Buffers
System.Memory
System.Numerics.Vectors
System.Runtime.CompilerServices.Unsafe

You can find the complete list in the packages.config file.

### **Usage**

Upon running the application, the user is prompted to choose one of the three functionalities. Depending on the choice, the user might need to provide additional inputs, such as location coordinates.

### **Input Files**

The application uses CSV files for input. Examples include:

street_input.csv for checking if a location is in a street.
speed_limit_input.csv for checking the speed limit of a location.
Configuration

The application is configured to run on .NET Framework version 4.7.2. You can find more details in the App.config file.
