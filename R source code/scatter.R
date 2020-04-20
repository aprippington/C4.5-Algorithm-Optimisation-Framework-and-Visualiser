#All iterations, show increase by increasing X 9randomising less frequently) up until we get to i=1000 
#where randomising slightly is better than not at all

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
data_12 = readLines("Heart_disease_results_i_1000_X_10.csv", skipNul=TRUE)
data_13 = readLines("Heart_disease_results_i_1000_X_50.csv", skipNul=TRUE)
data_14 = readLines("Heart_disease_results_i_1000_X_100.csv", skipNul=TRUE)
data_15 = readLines("Heart_disease_results_i_1000_X_500.csv", skipNul=TRUE)
data_16 = readLines("Heart_disease_results_i_1000_X_0.csv", skipNul=TRUE)

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
dat_13 <- read.csv(textConnection(data_13), header = TRUE)      #i=1000, x=50
dat_14 <- read.csv(textConnection(data_14), header = TRUE)     #i=1000, x=100
dat_15 <- read.csv(textConnection(data_15), header = TRUE)     #i=1000, x=0
dat_16 <- read.csv(textConnection(data_16), header = TRUE)     #i=1000, x=0

dat_1$condition <- "i=0"
dat_2$condition <- "i=10,x=2"
dat_3$condition <- "i=10,x=5"
dat_5$condition <- "i=10,x=0"
dat_6$condition <- "i=100,x=2"
dat_7$condition <- "i=100,x=5"
dat_8$condition <- "i=100,x=10"
dat_9$condition <- "i=100,x=0"
dat_10$condition <- "i=1000,x=2"
dat_11$condition <- "i=1000,x=5"
dat_12$condition <- "i=1000,x=10"
dat_13$condition <- "i=1000,x=50"
dat_14$condition <- "i=1000,x=100"
dat_15$condition <- "i=1000,x=500"
dat_16$condition <- "i=1000,x=0"

df <- rbind(dat_1,dat_2,dat_3,dat_5,dat_6,dat_7,dat_8,dat_9,dat_10,dat_11,dat_12,dat_13,dat_14,dat_15,dat_16)

gr <- ggplot(data=df, aes(x=condition, y=Average.Size))+ geom_line(stat='summary', fun.y='mean') +
         geom_point(stat='summary', fun.y='mean', cex = 3)
gr + scale_y_continuous(name="Average Size", limits=c(10,25), oob = scales::squish) 

gr + scale_x_discrete(name="Configuration", limits=c("i=0",
                                                     "i=10,x=2","i=10,x=5", "i=10,x=0",
                                                     "i=100,x=2","i=100,x=5","i=100,x=10",
                                                     "i=100,x=0","i=1000,x=2","i=1000,x=5","i=1000,x=10",
                                                     "i=1000,x=50","i=1000,x=100","i=1000,x=500","i=1000,x=0"))+ theme(axis.text.x = element_text(angle=90, hjust=0), text = element_text(size=20))

gr + ggtitle("Average tree size for all configurations (Heart Disease data)")
                                                    
                                                                                                                                               
