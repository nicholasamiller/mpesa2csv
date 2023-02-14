# mpesa2csv

What this does:
1. The Blazor WASM client collects an MPesa account statement PDF and password from the user using the browser's file upload API.
2. The Blazor WASM client posts the file and password to an Azure Function.
3. The Azure Function extracts the transactions from the PDF file. 
4. The Azure Function saves the transactions as a CSV to an Azure Storage Account.  It generates a secure access signature for the blob with an expiry date.
5. The Azure Function returns a URL link to file using the secure access signature.
6. The Blazor WASM client creates shows a dialog with a download link for the user to download.

Thus we can upload and download files without ever reading them into memory using using a javascript API.


## Setup

Create an Azure Key Vault.

API uses it to store connection string for Azure Storage.
```
New-AzKeyVault -Name "<your-unique-keyvault-name>" -ResourceGroupName "myResourceGroup" -Location "EastUS"
```


