# CCopy
minimal command line tool error free for Directory/File copy. Better for use in CI/CD pipelines

## Usage
ccopy -s `<source>` -d `<dest>`

## additional switches

-r | --replace (Overwrite existing files at the destination)

-a | --all (Copy subdirectories and preserve path to the destination)

-q | --quiet (No console messages)

-t | --types "*.jpg *.config" (Select particular filetypes from the source to copy)

## Sample
`ccopy -s publish\ts-api-gateway -d C:\Apps\sites\ms-dev\tsapi-gateway -r -q -t "*.dll *.exe *deps.json *runtimeconfig.json *-service-*.json ocelot.json web.config"`
