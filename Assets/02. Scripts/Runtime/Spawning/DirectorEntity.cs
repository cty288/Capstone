using Runtime.DataFramework.Entities;

namespace Runtime.Spawning
{
    public interface IDirectorEntity : IEntity
    {
    }

    public abstract class DirectorEntity : AbstractBasicEntity
    {
        [field: ES3Serializable] 
        public float minSpawnRange;
        
        [field: ES3Serializable] 
        public float maxSpawnRange;
        
        [field: ES3Serializable] 
        public int startingCredits;
        
        [field: ES3Serializable] 
        public int currentCredits;
        
        [field: ES3Serializable] 
        public int creditsPerSecond;
        
        [field: ES3Serializable] 
        public float spawnTimer;
        
        [field: ES3Serializable] 
        public float packSpawnTimer;
        
        [field: ES3Serializable] 
        public Lottery m_lottery;
        
        // [field: ES3Serializable] 
        // public LevelEntity LevelEntity;
    }
}