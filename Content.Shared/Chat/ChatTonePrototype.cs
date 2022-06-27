using Robust.Shared.Prototypes;

namespace Content.Shared.Chat
{
    /// <summary>
    ///     Defines a chat tone, with a Regular Expression and a paired LOC string
    /// </summary>
    [Prototype("chatTone")]
    public sealed class ChatTonePrototype : IPrototype
    {
        [ViewVariables]
        [IdDataFieldAttribute]
        public string ID { get; } = default!;

        /// <summary>
        ///     The Regular Expression in string form
        /// </summary>
        [DataField("regex")]
        public string Regex { get; set; } = default!;

        /// <summary>
        ///     The paired LOC string
        /// </summary>
        [DataField("paired")]
        public string Paired { get; set; } = default!;
    }
}
