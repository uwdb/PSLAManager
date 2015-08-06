# PSLAManager 

This is a prototype of the PSLAManager system, based on the paper [Changing the Face of Database Cloud Services with Personalized Service Level Agreements](http://myria.cs.washington.edu/publications/Ortiz_PSLA_CIDR_2015.pdf).  The key idea of the PSLA is for a user to specify the schema of her data and basic statistics (e.g., base table cardinalities) and for the cloud service to show what types of queries the user can run and the performance of these queries with different configurations (which we define as "tiers") of the service, each with a defined cost. 

## Setup
1. Clone the repository

2. Open the solution ```PSLADemoCode.sln``` in Visual Studio and build the project. This will generate the ```PSLADemoCode.exe``` in the solution path.

##  Running PSLAManager
When generating a PSLA, the program takes as input the following components:

* **Dataset Schema**: description about the user's dataset in which PSLAManager will use to generate a PSLA
* **Training Data**: Statistics on a separate synthetic dataset to learn about the performance of each tier 
* **Testing Data**: Statistics about the user's dataset which will help predict runtimes

PSLAManager first uses the **Dataset Schema** to automatically generate queries for the dataset. The runtimes for these queries are then predicted for different configurations of 
the service by using the **Training Data** and the **Testing Data**. As a final step, PSLAManager uses algorithms to compress the generated queries before demonstrating the final PSLA. 

#### Option #1 : Using Default Settings

Nothing needs to be modified for this option. As an example, we provide dataset information based on the [TPCH Star Schema Benchmark](http://www.cs.umb.edu/~poneil/StarSchemaB.PDF). In this setting, we predict runtimes for different configurations of the  [Myria](http://myria.cs.washington.edu/) cloud service (specifically for configurations of 4, 6, 8, and 16 workers).

Under the ```PSLAManager\PSLAFiles``` directory you are provided with all the necessary components:
  * **Dataset Schema**: the file ```SchemaDefinition.txt``` provides information about the TPC-H SSB schema. 
  * **Training/Testing Data**: Under the folder ```predictions_for_tiers```, we provide training and testing data for the different tiers of the Myria service. 

When running PSLAManager, click on the executable in the solution path.

Once the program is running, first select the "Generate a PSLA" option. Second, you will need to select between using "predicted runtimes" or "real runtimes (assuming perfect predictions)". The resulting PSLA will appear as ```PSLAManager\PSLAFiles\FinalPSLA.json```.

#### Option #2 : Customize the Schema and Testing/Training Datasets
In this option, you can customize the dataset, number of tiers, and testing/training data for an entirely different cloud service.

If you are providing a new schema, the statistics for the testing data must be updated to reflect on new queries generated by PSLAManager (according to the algorithm, the number of generated queries will change depending on the number of tables, attributes, sizes, etc. described in the schema). To customize PSLAManager, follow these steps:

* **Step 1: Edit the schema for your dataset** Under ```PSLAManager\PSLAFiles``` edit the ```SchemaDefinition.txt``` to reflect the new schema. Documentation on how to edit is in the file.

* **Step 2: Select New Tiers** This is optional, you can keep the default configurations used for the Myria service (4, 6, 8, and 16) or select your own. To do this, go to the ```PSLAManager\PSLAFiles\predictions_for_tiers``` folder and edit the ```tiers.txt``` file. Instructions are provided in the file.

* **Step 3: Collect Statisics for Training** You will need to update training data if you have 1) you have created new tiers or 2) you are training for a different cloud service besides Myria. Otherwise, you may skip this step.

    * **Supplementary Files** To help with the training, we provide both the queries and the data for a synthetic benchmark. You must collect statistics about this dataset for each tier you have defined in Step 3. The queries you need to collect statistics from are located under ```PSLAManager\PSLAFiles\predictions_for_tiers\custom_files\SQLQueries-Synthetic.txt```. In this directory we also provide the DDL for the synthetic dataset. You can download the data from the following S3 bucket: ```s3://synthetic-dataset```. 
    * **Examples for Training Statistics** Under ```PSLAManager\PSLAFiles\predictions_for_tiers\prediction_for_tiers``` you will see a directory for each tier as defined in Step 3. Inside each tier provided by default, there is a file called ```TRAINING.arff``` which provides an example of statistics collected for training.

* **Step 4: Collect Statisics for Testing** For each tier, you will also need to provide statistcs for the queries generated from your custom dataset.  
    
    * **Supplementary Files** You will first need PSLAManager to generate the queries (from your custom dataset) which you will be collecting statistics from. To do this, Run ```PSLAManager.exe``` and select the "Generate queries" option. This will create a file under ```PSLAManager\PSLAFiles``` called ```SQLQueries-Generated.txt```.
    *  **Examples of Testing**  Similarly to the  training data, you should see examples of testing statistics under ```PSLAManager\PSLAFiles\predictions_for_tiers\prediction_for_tiers``` under a tier folder with a file called ```TESTING.arff```.
     * **Optional (assuming perfect predictions)**:  If you would like to provide real runtimes for these different tiers and skip predictions altogether, for each  tier simply provide a ```realtimes.txt``` file with real runtimes for each query generated.

* **Step 5: Run PSLAManager** Before running PSLAManager to generate a PSLA, please ensure you have the following components ready:
    * A custom dataset defined under ```PSLAManager\PSLAFiles\SchemaDefinition.txt```
    * Modified tiers under ```PSLAManager\PSLAFiles\predictions_for_tiers\tiers.txt```
    * Modified Training/Testing data for each tier defined under ```PSLAManager\PSLAFiles\predictions_for_tiers```

Once ready, run the ```PSLAManager.exe``` and select the "Generate a PSLA" option. The resulting PSLA will appear as ```PSLAManager\PSLAFiles\FinalPSLA.json```.
