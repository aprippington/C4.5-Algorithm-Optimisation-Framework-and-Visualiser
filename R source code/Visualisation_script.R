install.packages("dplyr")
install.packages("plyr")
library(plyr)
library(dplyr)

data_1 = readLines("Heart_disease_results_i_0_X_0.csv", skipNul=TRUE)
data_2 = readLines("Heart_disease_results_i_10_X_2.csv", skipNul=TRUE)
data_3 = readLines("Heart_disease_results_i_10_X_5.csv", skipNul=TRUE)
data_5 = readLines("Heart_disease_results_i_10_X_0.csv", skipNul=TRUE)
data_6 = readLines("Heart_disease_results_i_100_X_2.csv", skipNul=TRUE)
data_7 = readLines("Heart_disease_results_i_100_X_5.csv", skipNul=TRUE)
data_8 = readLines("Heart_disease_results_i_100_X_10.csv", skipNul=TRUE)
data_9 = readLines("Heart_disease_results_i_100_X_0.csv", skipNul=TRUE)
data_10 = readLines("Heart_disease_results_i_1000_X_2.csv", skipNul=TRUE)
data_11 = readLines("Heart_disease_results_i_1000_X_5.csv", skipNul=TRUE)


dat_1 <- read.csv(textConnection(data_1), header = TRUE)      #i=0, x=n/a
dat_2 <- read.csv(textConnection(data_2), header = TRUE)      #i=10, x=2
dat_3 <- read.csv(textConnection(data_3), header = TRUE)      #i=10, x=5
dat_5 <- read.csv(textConnection(data_5), header = TRUE)      #i=10, x=0
dat_6 <- read.csv(textConnection(data_6), header = TRUE)      #i=100, x=2
dat_7 <- read.csv(textConnection(data_7), header = TRUE)      #i=100, x=5
dat_8 <- read.csv(textConnection(data_8), header = TRUE)      #i=100, x=10
dat_9 <- read.csv(textConnection(data_9), header = TRUE)      #i=100, x=0
dat_10 <- read.csv(textConnection(data_10), header = TRUE)     #i=1000, x=2
dat_11 <- read.csv(textConnection(data_11), header = TRUE)     #i=1000, x=5
dat_12 <- read.csv(textConnection(data_12), header = TRUE)     #i=1000, x=10
dat_13 < read.csv(textConnection(data_13), header = TRUE)      #i=1000, x=50
dat_14 <- read.csv(textConnection(data_14), header = TRUE)     #i=1000, x=100
dat_15 <- read.csv(textConnection(data_15), header = TRUE)     #i=1000, x=0

#Compare data distributions with a stable X value

dat_1$condition <- "Base C4.5 (No optimisation)"
dat_3$condition <- "i=10,x=5"
dat_7$condition <- "i=100,x=5"
dat_11$condition <- "i=1000,x=5"

df <- rbind(dat_1,dat_3,dat_7,dat_11)

library(ggplot2)
box <- boxplot(df$Test.Accuracy~condition,data=df, yaxt="n", main="Distribution of Test Accuracies for Heart Disease Data",
               xlab="Configuration", ylab="Test Accuracy (%)", col = "orange",
               border = "brown", ylim=c(30,80) ,xaxs="i", yaxs="i")

#Create custom y-axis
ticks<-seq(from = 30, to = 80, by = 10)
axis(2,at=ticks,labels=ticks)

#Add dotted horizontal lines
grid(nx = 0, ny = 8, col = "lightgray", lty = "dotted",
     lwd = par("lwd"), equilogs = TRUE)

#Add numbered labels for box statistics
text(x = col(box$stats)-0.1, y = box$stats + 0.4, labels = format(round(box$stats, 2), nsmall = 2), cex=1, col = 'black')

print(box)







