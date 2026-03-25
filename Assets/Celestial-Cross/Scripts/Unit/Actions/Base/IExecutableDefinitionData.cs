using System;

public interface IExecutableDefinitionData
{
    string DefinitionName { get; }
    Type GetRuntimeActionType();
    void Configure(IUnitAction action);
}
