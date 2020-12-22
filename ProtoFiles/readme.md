# gRPC Proto

This project is simply to hold the `.proto` file that is used to define the protobuf messages and service endpoints.
No class files or actual code should be used in this project. In order to generate the auto-generated classes for this project, simply
build the project. The auto-generated classes will be created. After building, any dependant projects will be updated to reflect any changes
in the `.proto` file.