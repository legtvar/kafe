dotnet dev-certs https
dotnet dev-certs https --trust
dotnet dev-certs https --export-path ./Web/certs/_dotnet.pem --no-password --format PEM
