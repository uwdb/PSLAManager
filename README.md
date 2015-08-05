# PSLAManager 

This prototype of the PSLAManager service is based on the paper [Changing the Face of Database Cloud Services with Personalized Service Level Agreements](http://myria.cs.washington.edu/publications/Ortiz_PSLA_CIDR_2015.pdf).  The key idea of the PSLA is for a user to specify the schema of her data and basic statistics (e.g., base table cardinalities) and for the cloud service to show what types of queries the user can run and the performance of these
queries with different levels of service, each with a defined cost.

## Setup
1. Clone the repository

2. Open the solution ```PSLADemoCode.sln``` in Visual Studio and build the project. This will generate the ```PSLADemoCode.exe``` in the solution path.

## Customize
There are several ways you can customize PSLAManager. The program takes as input the following components:

* **Dataset Schema**: description of the dataset
* **Training/Testing Data**: to predict runtimes for different configurations

PSLAManager first uses the **Dataset Schema** to automatically generate queries for the dataset. The runtimes for these queries are then predicted for different configurations of 
the service by using the **Training/Testing Data**. As a final step, PSLAManager uses algorithms to compress the queries before demonstrating the final PSLA. 

#### Option #1 : Use Default Settings

Nothing needs to be modified for this option. As an example, we provided dataset information based on the [TPCH Star Schema Benchmark](http://www.cs.umb.edu/~poneil/StarSchemaB.PDF). In this setting, we predict runtimes for different configurations of the  [Myria](http://myria.cs.washington.edu/) cloud service. 

Under the ```PSLADemoCode\PSLAFiles``` directory you are provided with all the necessary inputs:

  * **Dataset Schema**: the file ```SchemaDefinitionTPCH.txt``` provides information about the TPC-H SSB schema.
  * **Training/Testing Data**: Under the folder ```predictions```, we provide training and testing data for different configurations of the Myria service. 

#### Option #2 : Customize the Schema and Testing/Training Datasets
*TO DO*

## Running PSLAManager
*TO DO*