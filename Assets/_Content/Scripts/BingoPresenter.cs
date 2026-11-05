using System;

public sealed class BingoPresenter
{
    private readonly BingoSessionStore _store;
    public BingoModel Model { get; private set; }
    public event Action Changed;

    public BingoPresenter(BingoSessionStore store) { _store = store; }

    public void Open(BingoPreset preset)
    {
        Model = _store.Load(preset.BingoType) ?? BingoModel.Generate(preset, _store.NextNumber);
        _store.Save(Model);
        Changed?.Invoke();
    }

    public void Toggle(int index)
    {
        Model.Toggle(index);
        _store.Save(Model);
        Changed?.Invoke();
    }

    public void Finish()
    {
        if (Model == null) return;
        _store.Finish(Model.Session.type);
        Model = null;
    }
}
