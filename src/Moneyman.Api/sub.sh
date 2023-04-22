#!/bin/bash

# Define the paths to the files
appsettings_path="./local_appsettings.json"
new_settings_path="./appsettings.json"

# Read the contents of the files into variables
appsettings=$(cat $appsettings_path)
new_settings=$(cat $new_settings_path)

# Use jq to extract the key-value pairs that differ
changed_settings=$(echo $new_settings | jq --argjson old "$appsettings" -c 'to_entries | map(select(.value != ($old | getpath([.key]))))')

# If there are no changes, exit the script
if [ -z "$changed_settings" ]; then
  echo "No changes to appsettings"
  exit 0
fi

# Update the appsettings file with the changed key-value pairs
for row in $(echo "${changed_settings}" | jq -r '.[] | @base64'); do
  _jq() {
    echo ${row} | base64 --decode | jq -r ${1}
  }

  key=$(_jq '.key')
  value=$(_jq '.value')

  sed -i "s/\"$key\": .*$/\"$key\": $value,/" $appsettings_path
done

echo "Updated appsettings with new settings"
