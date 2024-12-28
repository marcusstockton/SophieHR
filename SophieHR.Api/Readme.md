## Useful Links:

- [Grafana](http://localhost:3000/)
- [Prometheus](http://localhost:9090/)
- [Kibana](http://localhost:5601/)


## Notes:

Uses Redis caching (currently only in CompanyController)  
HereApi seems to have introduced a new payment platform so need to find a new api for looking up postcodes and gettig info.

## TODO:

Replace all DTO's with Records.  
Remove automapper - do it my self seems to be the preferred way currently.


## Ideas:

Make it multi-tenant by using the company-id as the tenantid used in queries?  
https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy



## Migration gotchas...

Its a bit tricky, think of the database as seperate.  
You have to manage the ef stuff via the api project, so I open up a cmd from the api root and run my commands there passing in the connection string:  
`dotnet ef migrations add <blah>`
`dotnet ef database update --connection "Server=127.0.0.1,1433;Database=aspnet-SophieHR.Api;User=sa;Password=P@55w0rd123;MultipleActiveResultSets=true;TrustServerCertificate=true"`

