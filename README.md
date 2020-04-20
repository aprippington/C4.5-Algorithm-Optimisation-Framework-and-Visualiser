# (C#) C4.5 Algorithm, Optimisation Framework and Visualiser
This software is a generalised framework for inducing and optimising classification trees.
Given a dataset, we generate a decision/regression tree using C4.5 and subsequently optimise the tree structure with respect to accuracy and size. 

### Prerequisites

* Microsoft Visual Studio 2019 IDE
* Microsoft Visual Studio Code

#### Dependencies

Package Management:
``` 
Nuget                                        (https://www.nuget.org/)
```

Visualisation:
```
Emscripten                                   (https://emscripten.org)
Nodejs                                       (https://nodejs.org/en/)
Yarn                                         (https://yarnpkg.com/)
Graphviz                                     (https://www.graphviz.org/)
Graphviz VS Code .DOT Language Support       (https://github.com/Stephanvs/vscode-graphviz)
GraphViz VS Code Previewer                   (https://marketplace.visualstudio.com/items?itemName=joaompinto.vscode-graphviz)
GraphViz Browser Wrapper                     (https://github.com/mdaines/viz.js/)
DotNetGraph                                  (https://github.com/vfrz/DotNetGraph/tree/master/Sources/DotNetGraph)  
```

## Installing

Install all of the above dependencies from the links provided.

Clone the following files to a local directory:

* Decision_Tree_Optimisation_Generalised.csproj
* Accuracy.cs
* Branch.cs
* DataLoader.cs
* DecisionTree.cs
* DOT_file_generator.cs
* Node.cs
* Program.cs


#### Project configuration

Store DotNetGraph.csproj as a reference from the main project.  

ClosedXML; will be installed automatically upon first opening the project. (Used to store results and performance metrics in a .xlsx file)

### Running 

Set all of the following parameters to desired values;

* R - row limiting parameter                                                                                      Location: DecicionTree.cs line 37
* V - Validation set size set (as a proportion of the full dataset)                                               Location: DecicionTree.cs line 646
* i – the number of times to perform our optimisation cycle                                                       Location: DecicionTree.cs line 662
* X – number of optimisation iterations implemented before re-selecting a random subset of our validation set     Location: DecicionTree.cs line 670
* Vi - Validation subset size (as a proportion of the validation set)                                             Location: DecicionTree.cs line 774
* D – Dataset (path to CSV file where dataset to be classified is stored)                                         Location: DecicionTree.cs line 811 
* K – number of partitions to split our data into for k-fold cross validation (default: 10)                       Location: DecicionTree.cs line 811

* Results - desired name of output .xlsx file                                                                     Location: DecisionTree.cs line 881  
* Vis - desired name of output .dot file                                                                          Location: DOT_file_generator.cs line 129

Note that both the training and test set sizes, Tr and T, are implicitly determined by the value of K. 
I.e if K=10, Tr=90% and T=10%    or   if k=5, Tr=80% and T=20%.

To start the program, run Program.cs Main method

### Capabilities & Restrictions

Both continuous and categorical input variables are permitted. Data types will be automatically detected during parsing. Accepted Data Types: Double, Int, String

Input CSV file:
* Target attribute must be in the final column
* Values must be stored to a consistent number of decimal places, i.e. if a column contains 3.4, then the value 3 must be stored as 3.0
* Top row must contain headings (attribute names)

### Visualisation

After running the C4.5 and optimisation code, a .DOT file will be created and stored in the working directory. Open this file with Visual Studio Code, right-click in 
the console and select 'Preview to the side'. This will display a GraphViz generated image of the best decision tree created.

### Design
Design of this project can be broken down into two parts; Data structure selection and class architecture. 

The first critical decision that had to be made was selecting the data types and structures that would be used to represent our datasets. Initially, 2D arrays 
were selected as the dataset storage structure. However, this was almost immediately changed and the DataTable class was chosen instead as it provides in-built generalisation
power that 2D arrays could not offer- as well as forseeing vulnerability when altering a dataset comprised of arrays. We made this decision as we preferred to have a more
robust framework that we could manipulate safely rather than a faster, but more fragile structure comprised of arrays. Optimisation of a tree structure also requires a large 
amount of data cloning, as each node must store the dataset partition that 'enters' it. Utilising the DataTable class allows this functionality to be implemented in fewer lines 
of code, allowing our code to be more interpretable should modifications be required. 

The initial class structure is fairly intuitive. Most methods are either recursive (and thus stored in the Node class) or are stored in the DecisionTree class which contains
the C4.5 algorithm itself. Recursive methods are the foundation of this project as we are dealing with trees on a macro level rather than individual nodes, thus all methods that 
are implemented on tree structures themselves are stored within class Node. A manual method has been written to create deep clones of Nodes, at which point we are able to safely
manipulate trees without losing their initial structure. 

### Testing
Unit testing was performed as the project was built, generally on a function by function level. Each method has been sanity tested upon creation to ensure correct functionality. 
Integration tests were used where possible between pairs of related methods to ensure compatibility of data types and correctness, as well as to assess possible vulnerability
to unchecked exceptions. After the inital C4.5 algorithm was implemented, build verification testing was performed to ensure that the expected output was within a reasonable range or other similar
classifiers (in other languages).

Regression testing was performed towards the end of programming to ensure that any new additions did not affect pre-existing functionality. Finally, system testing was used to 
measure the capability of the framework for accepting input data of different formats. We've ensured that our code can identify and correctly parse/manage data of binary, continuous
and categorical/nominal types. 

### Versioning Statergy
Semantic Versioning  
1.0 Dataset-specific code (Abalone, no optimisation)  
2.0 Decision Tree Optimisation (Generalised)

[major.patch] will be used for future releases if they are to materialise.   
Major: Implemented for large changes which cause older versions to become redundant. i.e. All functionality remains but a new large feature is added.   
Patch: Bug fixes  

### Authors
* Andrew Rippington  

### Credits

@noffle for the 'Art of README' article which helped to form the basis this document.    
The following repository also helped inspire some small segments of code structure. Unknown author; Codeplex https://archive.codeplex.com/?p=id3algorithm

