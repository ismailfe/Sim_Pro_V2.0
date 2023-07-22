using System.ComponentModel;

namespace OplcE_Sim_Pro
{
    class Config
    {
        public BindingList<StationData> Stations = new BindingList<StationData>();        

        public bool IsStationNameUnique(string name)
        {
            foreach (StationData st in Stations)
            {
                if (st.Name == name)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsStationNameUniqueExcept(string name, int index)
        {
            for (int i = 0; i < Stations.Count; i++)
            {
                if (i != index)
                {
                    if (Stations[i].Name == name)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }    
}
