# Project Title: ML 24/25-01 Investigate Image Reconstruction by using Classifiers

## Table of Contents

- [Problem Statement](#problem-statement)
- [Introduction](#introduction)
- [Image Encoder](#image-encoder)
- [Sparse Distributed Representations (SDR)](#sparse-distributed-representations-sdr)
- [Hierarchical Temporal Memory (HTM)](#hierarchical-temporal-memory-htm)
- [Spatial Pooler (SP)](#spatial-pooler-sp)
  - [Spatial Pooler Functions and Mechanisms](#spatial-pooler-functions-and-mechanisms)
  - [Phases of the Spatial Pooler](#phases-of-the-spatial-pooler)
- [K-Nearest Neighbours (KNN) Classifiers](#k-nearest-neighbours-knn-classifiers)
- [HTM Classifiers](#htm-classifiers)
- [Difference between HTM and KNN Classifiers](#difference-between-htm-and-knn-classifiers)


### Problem Statement:
This project aims to explore the role of classifiers in Hierarchical Temporal Memory (HTM) systems,
focusing on their ability to associate input patterns with meaningful predictions and reconstruct
original inputs from Sparse Distributed Representations (SDRs). By investigating and comparing two
existing classifiers, HtmClassifier and KNN, the project seeks to evaluate their functionality,
performance, and differences. Inspired by the SpatialLearning experiment, a new experiment will be
implemented to regenerate input images from SDRs produced by the Spatial Pooler (SP), leveraging
the IClassifier interface for learning and prediction. The experiment will use the ImageEncoder to
process images, reconstruct inputs via classifiers, and compare them with the originals using
similarity measures. Results will be illustrated with diagrams, analysed quantitatively, and discussed,
providing insights into the reconstruction capabilities of classifiers in HTM systems and their
practical implications.

### Introduction:
This project explores the integration and application of classifiers within the Hierarchical Temporal Memory (HTM) framework to regenerate input data from Sparse Distributed Representations (SDRs). The core goal is to understand the role of classifiers in reverse encoding, where the learned SDR representations are used to reconstruct the original input. Through this process, we aim to analyse the behaviour and performance of two existing classifiers—HtmClassifier and KNN—and implement a new experiment that leverages their capabilities. The two classifiers under study in this project—HtmClassifier and KNN—serve as foundational implementations. The HtmClassifier leverages the principles of temporal memory within HTM, while the KNN classifier employs a distance-based approach to classify SDRs based on nearest neighbours. Through this investigation, the project bridges the gap between abstract HTM theories and practical applications, contributing to advancements in intelligent systems and neural computation.

### Image Encoder:
The ImageEncoder plays a crucial role in preparing image data for processing within Hierarchical
Temporal Memory (HTM) systems by converting raw image inputs into binary representations
compatible with HTM's Sparse Distributed Representations (SDRs). Based on the ImageBinarizer
NuGet package, the ImageEncoder encodes pixel intensity or feature information into a format that
preserves essential patterns while reducing redundancy. This encoding ensures that similar images
produce similar SDRs, a key characteristic that enables effective learning and pattern recognition in
HTM systems. By preprocessing images into this sparse binary format, the ImageEncoder bridges the
gap between raw image data and the HTM's Spatial Pooler, making it a foundational component for
image-based experiments, such as learning spatial patterns or regenerating inputs from SDRs.

###Encoding Process
####Image Binarization
Input images are binarized using thresholds for RGB channels and resized. Binarized images are saved for further processing.

####Binary Conversion
Binarized images are converted into a binary array (inputVector) for processing with the NeoCortex framework.

####Spatial Pooling
The binary input vector is processed using the Spatial Pooler to generate Sparse Distributed Representations (SDRs), identifying active columns.

####Labeling and Storage
SDRs are labeled with image names and stored for training and prediction in downstream tasks like classification.

####Visualization
1D heatmaps and similarity plots are generated for analysis and monitoring of the encoding process.

### Sparse Distributed Representations (SDR):

Sparse Distributed Representations (SDRs) are analogous to how the human brain encodes
information. Just as neurons in the brain fire in sparse patterns, with only a small fraction of neurons
active at any time, SDRs use binary vectors where a small percentage of bits are active (1s) while the
rest are inactive (0s). In the brain, these sparse activations ensure energy efficiency and robustness,
as the overlap in neural firing patterns helps identify similar stimuli. Similarly, SDRs are sparse to
reduce computational complexity and distributed to make the system resilient to noise or
corruption. Technically, SDRs preserve key properties of input data, such as similarity and
distinctiveness, through overlapping active bits for similar inputs and distinct patterns for dissimilar
inputs. This allows HTM systems to efficiently encode, recognize, and generalize patterns, just as the
brain does when processing sensory input. SDRs form the foundation for all processing stages in
HTM, including spatial pooling and temporal memory, providing a biologically plausible and
computationally robust framework for learning and prediction.

### Hierarchical Temporal Memory (HTM)

Hierarchical Temporal Memory is a theoretical framework and machine learning approach inspired by the structure and function of the neocortex in the human brain. Temporal Memory (TM) is a key component of HTM. It is the mechanism responsible for learning and predicting sequences of patterns based on temporal context. It is designed to capture and store the sequence in which events occur, allowing the system to recognize and predict temporal patterns. The workflow of Hierarchical Temporal Memory (HTM) involves processing input data into sparse distributed representations (SDRs) using an encoder, which captures the essential features of the input. These SDRs are passed to the spatial pooler, which ensures sparsity and generalizes similar inputs. The temporal memory then learns and stores temporal sequences by activating specific cells in its structure based on previous contexts, forming associations over time. As new inputs arrive, the temporal memory predicts future sequences by activating likely next-step cells based on learned patterns. 

### Spatial Pooler (SP):

The Spatial Pooler is a fundamental component of Hierarchical Temporal Memory (HTM) systems, transforming raw input data into Sparse Distributed Representations (SDRs). Its primary function is to encode the input while ensuring key properties such as sparsity and similarity preservation. Sparsity ensures that only a small percentage of bits in the SDR are active, which improves computational efficiency and reduces noise sensitivity. Similarity preservation means that inputs with similar patterns produce SDRs with overlapping active bits, enabling the system to recognize related patterns effectively. The Spatial Pooler achieves this through competition among columns of cells, where each column competes to represent specific input features, guided by synaptic connections that adapt over time. This adaptation allows the Spatial Pooler to learn the statistical structure of the input space, making it robust to noise and capable of generalizing from limited data. As a result, the Spatial Pooler provides the foundation for further processing, such as temporal learning and classification, in HTM systems.

### Spatial Pooler Functions and Mechanisms

#### Core Functions:
1. **Input Processing**:
   - Accepts spatially encoded patterns, such as binary vectors from the ImageEncoder.
   - Prepares inputs for compatibility with HTM's SDR format.

2. **Sparse Distributed Representations (SDR) Generation**:
   - Produces sparse binary outputs with a controlled percentage of active bits.
   - Ensures similar inputs yield SDRs with overlapping active bits, while dissimilar inputs produce distinct representations.

3. **Learning and Adaptation**:
   - Learns the statistical properties of input patterns via synaptic adaptation.
   - Dynamically adjusts synaptic permanence to optimize connectivity.

4. **Noise Resilience**:
   - Generalizes input patterns to recognize noisy or incomplete data.
   - Maintains robustness against input variations.

5. **Stability and Plasticity**:
   - Balances stability and flexibility in representations, ensuring consistent encoding of recurring patterns while adapting to new inputs.



### Phases of the Spatial Pooler:

1. **Overlap Phase**:
   - **Purpose**: Compute overlap between input patterns and synaptic connections for each column.
   - **Process**: Columns assess the similarity of their synapses to the input; higher overlap values indicate better matches.

2. **Inhibition Phase**:
   - **Purpose**: Ensure sparsity by limiting active columns.
   - **Process**: Columns compete within their inhibition radius, and only the top percentage of columns with the highest overlap are activated.

3. **Learning Phase**:
   - **Purpose**: Adapt synaptic connections to improve encoding of input patterns.
   - **Process**:
     - Active columns strengthen their synapses.
     - Synaptic permanence values are updated based on input activity and learning thresholds.
     - Homeostatic mechanisms promote balanced column utilization.

4. **Output Generation**:
   - **Purpose**: Produce high-quality SDRs for downstream processing.
   - **Process**: Converts learned patterns into robust, sparse binary vectors.

5. **Stability Monitoring**:
   - **Purpose**: Maintain system stability during learning.
   - **Process**: Adjusts parameters dynamically to balance plasticity and stability.


### HTM Classifiers:
Hierarchical Temporal Memory (HTM) classifiers play a crucial role in sequence learning and pattern recognition within the HTM framework. They are designed to associate Sparse Distributed Representations (SDRs) generated by the Spatial Pooler and Temporal Memory with meaningful labels, allowing the system to make predictions based on learned patterns. The HTM classifier works by mapping input SDRs to their corresponding output labels during the learning phase. Once trained, it can retrieve the most likely labels when presented with new or partial input patterns, making it particularly effective for recognizing temporal sequences and structured data. Unlike traditional classifiers, HTM classifiers leverage the biological principles of the neocortex, enabling them to handle noise, detect anomalies, and generalize from sparse inputs. Their ability to incrementally learn without requiring retraining makes them well-suited for real-time applications, such as image recognition, anomaly detection, and time-series forecasting. By continuously refining associations between SDRs and labels, HTM classifiers contribute to the adaptability and robustness of HTM-based systems.

### K-Nearest Neighbours (KNN) Classifiers:
The K-Nearest Neighbours (KNN) classifier is a simple, non-parametric algorithm used for classification and regression tasks. It stores all the labeled training data and classifies new data points based on their similarity to the closest training samples. When a new input is provided, the algorithm computes its distance (commonly using Euclidean distance) from all training points, identifies the "k" nearest neighbours, and assigns the most common label among those neighbours to the input. In the context of this project, KNN could serve as a baseline classifier or a comparative model for evaluating SDR representations. When provided with an SDR or a derived feature vector, KNN computes distances (e.g., Euclidean) to its "k" closest neighbours and predicts the most frequent label among them. This method can be useful in this project to classify SDRs or reconstructed patterns, allowing comparisons between HTM's ability to generalize patterns and KNN's reliance on proximity and similarity. While KNN is straightforward and effective for small-scale problems, it lacks the adaptive learning and biological inspiration of HTM, making it less dynamic for processing evolving data streams.

### Difference between HTM and KNN Classifiers:
The core difference between Hierarchical Temporal Memory (HTM) and K-Nearest Neighbors (KNN) lies in their approach to learning, adaptability, and the ability to handle high-dimensional data like Sparse Distributed Representations (SDRs). When analyzed in the context of this project, these differences highlight the strengths and weaknesses of each model in processing binarized image data and reconstructing patterns.

#### Learning Paradigm:
HTM is a biologically inspired model designed to mimic how the human brain processes spatial and temporal data. It adapts to input patterns dynamically, learning spatial relationships in the input data over time through unsupervised mechanisms. In this project, HTM's spatial pooler captures the structure of SDRs from binarized images and reconstructs them efficiently, showing its capacity to learn and generalize patterns dynamically. In contrast, KNN is a non-parametric supervised learning algorithm that relies on labeled data for training. KNN does not "learn" patterns or adapt to new data over time; it simply compares new input data to its neighbors in the feature space, making it unsuitable for tasks requiring pattern reconstruction or adaptation without explicit labels.

#### Adaptability:
HTM excels in its ability to continuously adapt to new inputs while preserving previously learned patterns. This makes it ideal for the iterative process of improving pattern recognition and reconstruction in this project. On the other hand, KNN does not adapt to new data unless the dataset is explicitly updated. This static nature makes it less suitable for evolving or streaming data scenarios, which are central to HTM's design.

#### Handling High-Dimensional Data:
The project deals with binary SDRs, which are high-dimensional and sparse by nature. HTM's design explicitly handles such data effectively by focusing on sparse, distributed representations. KNN, on the other hand, suffers from the "curse of dimensionality," where the distance metrics used for classification become less meaningful as dimensionality increases. This makes KNN less efficient and accurate when dealing with SDRs compared to HTM.

#### Reconstruction of Input Patterns:
HTM's spatial pooler not only classifies input patterns but also reconstructs them, a key requirement in this project. It achieves this by leveraging the learned synaptic connections and activation patterns. KNN lacks any mechanism for reconstructing input patterns since it is purely a classification algorithm, relying only on proximity-based voting.

#### Similarity Metrics and Interpretability:
In this project, HTM employs similarity metrics like the Jaccard Index to evaluate the overlap between the original and reconstructed SDRs. This interpretability and quantitative measure of learning are inherent to HTM. In contrast, KNN's interpretability is limited to understanding which neighbors influence a classification decision, and it does not contribute to understanding the underlying structure of the data.
