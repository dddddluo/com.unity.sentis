# Run a model

After you [create a worker](create-an-engine.md), use [`Execute`](xref:Unity.Sentis.IWorker.Execute) to run the model.

```
worker.Execute(inputTensor);
```

When you run a model for the first time in the Unity Editor, it might be slow because Sentis needs to compile code and shaders. Later runs are faster.

Refer to the `Run a model` example in the [sample scripts](package-samples.md) for an example.

## Additional resources

- [Run a model a layer at a time](run-a-model-a-layer-at-a-time.md)
- [Understand models in Sentis](models-concept.md)
- [Create an engine to run a model](create-an-engine.md)
- [Profile a model](profile-a-model.md)
