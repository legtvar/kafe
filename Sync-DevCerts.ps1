dotnet dev-certs https
dotnet dev-certs https --trust
dotnet dev-certs https --export-path ./Web/certs/ --no-password --format PEM
