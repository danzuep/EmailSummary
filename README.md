# Email Summary

This example uses [MailKitSimplified](https://github.com/danzuep/MailKitSimplified) and [LM-Kit.NET](https://lm-kit.com/products/lm-kit-net/). The code has been modified from their [document_summarizer](https://github.com/LM-Kit/lm-kit-net-samples/blob/main/console_net/document-intelligence/document-summarization/document_summarizer/Program.cs) example.

To test, start by running an email dev container like [smtp4dev](https://github.com/rnwood/smtp4dev) (e.g. `docker run -d -p 80:80 -p 25:25 -p 143:143 rnwood/smtp4dev`). Set it up with some sample emails.

Select your LM-Kit.NET model (e.g. "1"), then this demo will download the latest email and summarize it for you. It can take many seconds depending on your hardware. Once it finishes one, it will ask if you want to download the next email and summarize that, and so on and so forth until either they run out or you cancel it with `Ctrl+C`.
