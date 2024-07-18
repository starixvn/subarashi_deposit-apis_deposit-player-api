# Deposit Player API

The Deposit Player API is a web API for handling first deposits, shadow accounts and related tasks.

## Documents

- [Licensee API](https://docs.google.com/document/d/1LIUc3dI4gx4Q10Ea4GULX7yxT4gKGFdYWZPhsAxaKUE/edit#heading=h.oq9e9zptycc8)

## Notes

- [Apply Conventional Commits](https://gitlab.nexdev.net/nexdev/documentation/-/blob/master/Apply%20Conventional%20Commits.md)

## Built with

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Run Load Tests

- [Install K6](https://k6.io/docs/get-started/installation)
- Go to test folder
```shell
cd tests/LoadTests
```
- Run load tests
```shell
$env:K6_WEB_DASHBOARD="true" ; $env:K6_WEB_DASHBOARD_EXPORT="./Reports/[file-test-name].html" ; k6 run [file-test-name].js

or

K6_WEB_DASHBOARD=true K6_WEB_DASHBOARD_EXPORT="./Reports/[file-test-name].html" k6 run [file-test-name].js
```