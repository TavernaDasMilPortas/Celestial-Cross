public enum ActionState
{
    Idle,               // ação não ativa
    SelectingTargets,   // escolhendo alvos / tiles
    ReadyToConfirm,     // tudo escolhido, aguardando ENTER
    Resolving,          // pipeline rodando
    Finished            // ação concluída
}
