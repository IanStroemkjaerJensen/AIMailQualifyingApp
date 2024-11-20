namespace Care.Web.Domain.Models.OpenAi;

public class TokenizerData
{
    /// <summary>
    /// SpecialTokens: 
    /// </summary>
    public Dictionary<string, int> SpecialTokens { get; set; }
    /// <summary>
    /// Inputs prompts + output promps + extra tokens subtracted
    /// </summary>
    public int ModelsContextWindow { get; set; }

    /// <summary>
    /// ExtraTokensSubtracted: Extra tokens subtracted. May have something to do with SpecialTokens
    /// </summary>
    public int ExtraTokensSubtracted { get; set; }
}