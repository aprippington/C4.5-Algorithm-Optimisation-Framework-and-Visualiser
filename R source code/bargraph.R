library("reshape2")

data_10 = readLines("Heart_disease_results_i_1000_X_2.csv", skipNul=TRUE)
data_11 = readLines("Heart_disease_results_i_1000_X_5.csv", skipNul=TRUE)
data_12 = readLines("Heart_disease_results_i_1000_X_10.csv", skipNul=TRUE)
data_13 = readLines("Heart_disease_results_i_1000_X_50.csv", skipNul=TRUE)
data_14 = readLines("Heart_disease_results_i_1000_X_100.csv", skipNul=TRUE)
data_15 = readLines("Heart_disease_results_i_1000_X_500.csv", skipNul=TRUE)
data_16 = readLines("Heart_disease_results_i_1000_X_0.csv", skipNul=TRUE)

dat_10 <- read.csv(textConnection(data_10), header = TRUE)     #i=1000, x=2
dat_11 <- read.csv(textConnection(data_11), header = TRUE)     #i=1000, x=5
dat_12 <- read.csv(textConnection(data_12), header = TRUE)     #i=1000, x=10
dat_13 <- read.csv(textConnection(data_13), header = TRUE)     #i=1000, x=50
dat_14 <- read.csv(textConnection(data_14), header = TRUE)     #i=1000, x=100
dat_15 <- read.csv(textConnection(data_15), header = TRUE)     #i=1000, x=500
dat_16 <- read.csv(textConnection(data_16), header = TRUE)     #i=1000, x=0

dat_10$condition <- "i=1000,x=2"
dat_11$condition <- "i=1000,x=5"
dat_12$condition <- "i=1000,x=10"
dat_13$condition <- "i=1000,x=50"
dat_14$condition <- "i=1000,x=100"
dat_15$condition <- "i=1000,x=500"
dat_16$condition <- "i=1000,x=0"

df <- rbind(dat_10,dat_11,dat_12,dat_13,dat_14,dat_15,dat_16)
keeps <- c("Training.Accuracy", "Validation.Accuracy", "Test.Accuracy", "condition")
df <- df[keeps]
df.long<-reshape2::melt(df)

sp <- ggplot(df.long,aes(condition,value,fill=variable),ylim=c(67.5,72.5),main="Average Accuracy of i=1000")+ geom_bar(stat="summary",position="dodge", fun.y = "mean")
sp + scale_y_continuous(name="Average Accuracy", limits=c(67.5, 72.5), oob = scales::squish) + scale_x_discrete(name="Configuration", limits=c("i=1000,x=2","i=1000,x=5","i=1000,x=10","i=1000,x=50","i=1000,x=100",
                                                     "i=1000,x=500","i=1000,x=0"))+ title("Average Accuracy of i=1000")


#Add dotted horizontal lines
grid(nx = 0, ny = 5, col = "lightgray", lty = "dotted",
     lwd = par("lwd"), equilogs = TRUE)


#Compare i=1000 accuracies for varying X
#do this when i=1000, x= 2 and x=5 are ready...
