FROM microsoft/dotnet:1.1-runtime

WORKDIR /app
COPY /bin/Debug/netcoreapp1.1/publish/ .

ENTRYPOINT ["dotnet", "Server.dll"]
CMD ["datdb.cphbusiness.dk","rpc_queue_djur"]