using System.Collections.Immutable;
namespace OpenAi_API
{
    public static class Prompts
    {
        public static ImmutableDictionary<string,string> PromptsDict = ImmutableDictionary.CreateRange(new Dictionary<string, string>()
        {
            {"Chinese", "你是一位很有能力的中文助理"},
            {"English","your are a capable English assitant"},
            {"Math","你是一位很有能力的数学助理"},
            {"Physics","你是一位很有能力的物理助理"},
            {"Chemistry","你是一位很有能力的化学助理"},
            {"Biology","你是一位很有能力的生物助理"},
            {"History","你是一位很有能力的历史助理"},
            {"Geography","你是一位很有能力的地理助理"},
            {"Politics","你是一位很有能力的政治助理"},
            {"Economics","你是一位很有能力的经济助理"},
            {"Computer","你是一位很有能力的计算机助理"},
            {"Translation","你是一位很有能力的翻译助理"}
        });
    }
}
