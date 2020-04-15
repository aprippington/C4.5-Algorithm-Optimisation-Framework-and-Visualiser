# Optimisation of Classification Tree Models
This piece of software is a generalised framework for inducing and optimising classification trees.
Given a dataset, we generate a decision/regression tree and then optimise the tree structure with respect to accuracy and size. 

### Prerequisites

* Microsoft Visual Studio 2019 IDE
* Microsoft Visual Studio Code


* CSV file of data to be classified
* Restrictions:
* -Target attribute must be in the final column
* -Values must be to a consistent number of decimal places, i.e. if a column contains 3.4, then the value 3 must be stored as 3.0
* -Top row must contain headings (attribute names)

#### Download Dependencies

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

Clone the following files to a local directory:

*Decision_Tree_Optimisation_Generalised.csproj
*Accuracy.cs
*Branch.cs
*DataLoader.cs
*DecisionTree.cs
*DOT_file_generator.cs
*Node.cs


#### Project configuration

The Decision_Tree_Optimisation_Generalised.csproj file already contains correct configuration and references between projects, thus no manual configuration is necessary. 

ClosedXML; will be installed automatically upon first opening the project.

### Running 

* Parameters are stored at the following locations:
* 



### Versioning Statergy
[semantic versioning](https://semver.org/)

## Authors
* Andrew Rippington

## References
* [Gitlab Markdown Guide](https://docs.gitlab.com/ee/user/markdown.html)
* [Example 1](https://github.com/erasmus-without-paper/ewp-specs-sec-intro/tree/v2.0.2)
* [Example 2](https://github.com/erasmus-without-paper/ewp-specs-architecture/tree/v1.10.0)