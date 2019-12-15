namespace TurnOnLight
{
    /// <summary>
    /// Classe per modellare gli oggetti restituiti dal JSON di LUIS
    /// </summary>
    public class Analyzer
    {
        public Intent Intent { get; set; }
        public Entity Entity { get; set; }

    }

    /// <summary>
    /// Enum per modellare i possibili valori dell'oggetto "Entity" restituito da LUIS
    /// </summary>
    public enum Entity
    {
        // office
        Green,
        // kitchen
        Yellow,
        AllLights
    }

    /// <summary>
    /// Enum per modellare i possibili valori dell'oggetto "Intent" restituito da LUIS
    /// </summary>
    public enum Intent
    {
        TurnOn,
        TurnOff,
        None
    }

}
