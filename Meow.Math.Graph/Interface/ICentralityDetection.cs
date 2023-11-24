namespace Graph.Interface
{
    public interface ICentralityDetection
    {
        void DegreeCentrality();
        void DegreeAverage();
        void DegreeDistribution();
        void DegreeWeightedCentrality();
        void ClosenessCentrality();
        void HarmonicCentrality();
        void BetweennessCentrality();
        void PageRank();
    }
}
