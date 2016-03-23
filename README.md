# Marketing Tools - Testing

## Development environment 
### Precondition
#### NuGet Package Restore
We make use of the NuGet *package restore* functionality to fetch our dependencies, for this to work properly the following is required in the NuGet configuration:
* The following package sources are needed:
    * https://www.nuget.org/api/v2/
    * http://nuget.episerver.com/feed/packages.svc/
* To make use of both package sources at the same time select *All* as the *Package Source*
* The *Package Restore* setting needs to be enabled


### Development Environment Setup
1. Run setup script in the root folder in order to clean, build and configure development environment:
```
setup
```
2. Run /samples/EPiServer.Templates.Alloy test site in Visual Studio.

### How to
#### Upgrading EPiServer packages in the solution
0. Clean your environment to make sure that no garbage is committed after update:
```
clean
```
1. Remove/comment the following line in /samples/.gitignore file to make sure that changes in Alloy project can be committed:
```
EPiServer.Templates.Alloy/*
```
2. Update Episerver packages in the solution and rebuild.
3. Run the following command in VS Package Manager Console in order to update test database:
```
Update-EPiDatabase
```
4. Copy database file from /samples/EPiServer.Templates.Alloy/App_Data to /build/resources folder. Don't run Alloy site before doing it.
5. Commit your changes and discard editing of /samples/.gitignore