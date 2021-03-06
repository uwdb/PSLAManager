    CREATE TABLE FACT	  ( FACT_FK1    INTEGER NOT NULL,
						FACT_FK2    INTEGER NOT NULL,
						FACT_FK3    INTEGER NOT NULL,
						FACT_FK4    INTEGER NOT NULL,
						FACT_FK5    INTEGER NOT NULL,
						FACT_ATT6    INTEGER NOT NULL,
						FACT_ATT7    INTEGER NOT NULL,
						FACT_ATT8    INTEGER NOT NULL,
						FACT_ATT9    INTEGER NOT NULL,
						FACT_ATT10    INTEGER NOT NULL,
						FACT_ATT11    INTEGER NOT NULL,
						FACT_ATT12    INTEGER NOT NULL,
						FACT_ATT13    INTEGER NOT NULL,
						FACT_ATT14    INTEGER NOT NULL,
						FACT_ATT15    INTEGER NOT NULL,
						FACT_ATT16    INTEGER NOT NULL,
						FACT_ATT17    INTEGER NOT NULL,
						FACT_ATT18    INTEGER NOT NULL,
						FACT_ATT19    INTEGER NOT NULL,
						FACT_ATT20    INTEGER NOT NULL);
						
						
CREATE TABLE DIMENSION1 (DIM_PK1_1    INTEGER NOT NULL PRIMARY KEY, 
						 DIM_ATT2_1    VARCHAR(50) NOT NULL, 
						 DIM_ATT3_1    INTEGER NOT NULL, 
						 DIM_ATT4_1    VARCHAR(100) NOT NULL);
                             
CREATE TABLE DIMENSION2 (DIM_PK1_2    INTEGER NOT NULL  PRIMARY KEY, 
						 DIM_ATT2_2    VARCHAR(50) NOT NULL, 
						 DIM_ATT3_2    VARCHAR(50) NOT NULL, 
						 DIM_ATT4_2    INTEGER NOT NULL, 
						 DIM_ATT5_2   INTEGER NOT NULL, 
						 DIM_ATT6_2    VARCHAR(100) NOT NULL);

CREATE TABLE DIMENSION3 (DIM_PK1_3    INTEGER NOT NULL  PRIMARY KEY, 
						 DIM_ATT2_3    VARCHAR(50) NOT NULL, 
						 DIM_ATT3_3    VARCHAR(50) NOT NULL, 
						 DIM_ATT4_3    VARCHAR(50) NOT NULL, 
						 DIM_ATT5_3    INTEGER NOT NULL, 
						 DIM_ATT6_3    INTEGER NOT NULL, 
						 DIM_ATT7_3    INTEGER NOT NULL, 
						 DIM_ATT8_3    VARCHAR(100) NOT NULL);		
						 
CREATE TABLE DIMENSION4 (DIM_PK1_4    INTEGER NOT NULL PRIMARY KEY, 
						 DIM_ATT2_4    VARCHAR(50) NOT NULL, 
						 DIM_ATT3_4    VARCHAR(50) NOT NULL, 
						 DIM_ATT4_4    VARCHAR(50) NOT NULL, 
						 DIM_ATT5_4    VARCHAR(50) NOT NULL, 
						 DIM_ATT6_4    INTEGER NOT NULL, 
						 DIM_ATT7_4    INTEGER NOT NULL, 
						 DIM_ATT8_4    INTEGER NOT NULL, 
						 DIM_ATT9_4    INTEGER NOT NULL, 
						 DIM_ATT10_4   VARCHAR(100) NOT NULL);					 
						 
						 
CREATE TABLE DIMENSION5 (DIM_PK1_5     INTEGER NOT NULL  PRIMARY KEY, 
						 DIM_ATT2_5    VARCHAR(50) NOT NULL, 
						 DIM_ATT3_5    VARCHAR(50) NOT NULL, 
						 DIM_ATT4_5    VARCHAR(50) NOT NULL, 
						 DIM_ATT5_5    VARCHAR(50) NOT NULL, 
						 DIM_ATT6_5    VARCHAR(50) NOT NULL, 
						 DIM_ATT7_5    INTEGER NOT NULL, 
						 DIM_ATT8_5    INTEGER NOT NULL, 
						 DIM_ATT9_5    INTEGER NOT NULL, 
						 DIM_ATT10_5    INTEGER NOT NULL, 
						 DIM_ATT11_5    INTEGER NOT NULL, 
						 DIM_ATT12_5    VARCHAR(100) NOT NULL);						 
						 