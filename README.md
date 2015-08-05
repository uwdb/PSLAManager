# PSLAManager 

In this repository, we demo the PSLAManager service, based on [Changing the Face of Database Cloud Services with Personalized Service Level Agreements](http://myria.cs.washington.edu/publications/Ortiz_PSLA_CIDR_2015.pdf).  The key idea of the PSLA is for a user to specify the schema of her data and basic statistics (e.g., base table cardinalities) and for the cloud service to show what types of queries the user can run and the performance of these
queries with different levels of service, each with a defined cost.

In this demo, we provide statistics from the [Myria](http://myria.cs.washington.edu/) cloud service. The schema is based on the [TPCH Star Schema Benchmark](http://www.cs.umb.edu/~poneil/StarSchemaB.PDF). When running this program, PSLAManager will read the schema, generates queries for the dataset, and predicts how the queries will perform for different configurations of the Myria service. For this demo, the service predicts runtimes for 4, 6, 8 and 16 workers. The user has an option whether to use predicted runtimes or real runtimes. 

### Running PSLAManager
1. Once cloning the repository, open the solution ```PSLADemoCode.sln``` in Visual Studio and build the project. This will create the ```PSLADemoCode.exe``` in the solution path in the ```bin``` folder.

2. Once running the executable, You will need to type in "P" or "R" depending on whether you wish to run use predicted or real query runtimes and then press *Enter*.

3. Once finished, go to the ```PSLADemoCode\PSLAFiles``` directory and see the PSLA result under the ```FinalPSLA.json``` file. For this demo we only provide clustering through the "LogHuman" approach as shown in the paper. In this directory, you can also find information about the dataset schema, the queries generated, and the training data used for predicting query runtimes. 