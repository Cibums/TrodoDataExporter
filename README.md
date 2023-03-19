# Trodo.se Scraper Data Handling
This project is a web API that handles and returns data about products found on Trodo.se.

## How
Zyte.com's automatic extraction API is used to scrape trodo.se for its collection and store all of the product data as an object in an AWS S3 bucket. This data is then retrieved and deserialized by the API application before it is handled and then returned by a few GET endpoints.

## Endpoints
There are four endpoints in this API. The endpoints in the category controller use the same product data as the product controller. This means that no scraper is scraping the category data of trodo.se. Instead, the API is figuring out what categories exist by handling the product data.

### Get Products
It gets a JSON of all products from Trodo.se.

*This endpoint is available as both simplified and extended. Simplified is a simplified model that only contains the data we usually use. Extended is the model that is directly gotten from Zyte.com*

### Get Products Filtered
It gets a JSON of all products from Trodo.se, but it's filtered through several criteria. So you can, for example, choose to only return products from a specific manufacturer or within a certain price range.

*This endpoint is available as both simplified and extended. Simplified is a simplified model that only contains the data we usually use. Extended is the model that is directly gotten from Zyte.com*

### Get Category Tree
It gets a JSON of the whole category tree of the website. 

### Get the Most Specific Categories
It gets a list of the smallest/most specific categories of Trodo.se.

## Optimizations
Every method that gets anything via the network is cached for 1 hour after getting it once. This means that only the first call to any endpoint will take the time it needs to get and deserialize the object from the AWS S3 bucket. As a result, the second time you use any of the API's endpoints, the response will be almost instant. 

### Economic Advantages
There usually are economic advantages to optimizing code. For example, this project uses services (Zyte and AWS) that are paid for per request. This means that every time we can skip going through these services by caching the response, we decrease the system's response time and maintenance cost.

### Improvements
There is improvement potential in this project's optimization field (there always is). Depending on how big the S3 bucket's object is (how many products have been scraped from Zyte), the longer the first call to any endpoint will take. Even though the data is later saved in the cache, this is a problem because if the first call fails to finish, it also fails to set the cache, which means the API is unusable. The initial call to the API, therefore, has to be optimized too. Exactly how this can be done has to be analyzed, but some ideas are pagination, compression, or adding a loading bar to let the user know that the call has not stopped working.

## Unit tests
I'm a strong believer and advocate for the agile testing pyramid framework.

![image](https://user-images.githubusercontent.com/56683094/226146177-678dcb74-e783-489b-bbdc-c6fa80648aac.png)

I believe in high code coverage to make QA engineers' lives easier. Furthermore, even though each ticket will take a little longer, the time saved by preventing incidents and decreasing the time the QA has to spend on each ticket will reduce the total development time for a project. Therefore, I have also included many unit tests in this project.

## Deployment
This project is deployed publically at:

https://trodo-data-exporter-web-api.azurewebsites.net/swagger/index.html

The production build process and deployment are set up automatically with Github Actions. When the main branch is updated, the app is automatically deployed to an Azure environment. I decided to use Azure, because I personally think it is easier to work with when it comes to deployment, even though I use AWS in other parts of this project.

## Other
A final thing to mention is that this is a small project, but it has been designed with scalability in mind. Therefore, it's built using best practices, and as separated as possible from the beginning, so it's easy to continue building on.
