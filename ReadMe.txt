Hello

Project that  used Azure Storage services and Azure Function,

.Net Core 7.0 has also been developed.
The development process started with the Azure portal and continued with the Azure Storage Explorer,
which can be used locally due to increasing requests.
There is a web application project where requests are made, a library project for azure storage services, 
and an azure function project that will be triggered when the request is received.
Configured with Queue Trigger in Azure Function.
Azure table storage, which is a derivative of nosql database, is used as database.
Azure Storage Queue was used for the Queue needed in the project scenario.
The project was developed as NLayerArchitecture and coded in accordance with SOLID.

Scenario:
When the project is up, people are added to the project at http://localhost:4037/person. In the second step,
For the listed contacts, phone numbers are added to the contacts from the Add Number button at http://localhost:4037/file.
Then, an excel creation request is sent with the Create File button.
At the same time, the requested report record is inserted into the report table as state I creating.
The request is written to the Azure Storage Queue.
Azure Function, which is triggered only when data comes to the queue, is triggered when the request comes, 
and after creating the requested excel, it pulls the status of the relevant record in the report table,
the created excel is uploaded to the azure blob storage and informs the user with SignalR. 
The information falls in front of the user, the user who clicks on the link is directed to http://localhost:4037/blobs/ and 
sees the resulting excels. You can download excels with the download button or delete them with the delete button.

Thank you