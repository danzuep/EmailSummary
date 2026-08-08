# Setup Steps

Create the project:

```pwsh
# winget install --id Microsoft.DotNet.SDK.10 -e
# winget install --id Git.Git -e
# winget install --id github.cli -e
# cd C:\Source\danzuep
$orgName = "danzuep"
$projectName = "EmailSummary"
$gitUrl = "https://github.com/${orgName}/${projectName}.git"
$projectDescription = "Summarize emails using local NLP."
# mkdir $projectName
# cd $projectName
git init; #git branch -M main; git add .; git commit -m "Initial commit"
gh repo create --source=. --private --description $projectDescription # --confirm
git push -u origin main
mkdir src
cd src
dotnet new console -n $projectName
cd $projectName
dotnet add package "MailKitSimplified.Sender"
dotnet add package "MailKitSimplified.Receiver"
dotnet add package "LM-Kit.NET"
dotnet add package "LM-Kit.NET.Backend.Cuda13.Windows"
cd ..
dotnet new sln -n $projectName
dotnet sln add "${projectName}/${projectName}.csproj"
```

Run an email dev container like [smtp4dev](https://github.com/rnwood/smtp4dev) with `docker run -d -p 80:80 -p 25:25 -p 143:143 rnwood/smtp4dev`.
