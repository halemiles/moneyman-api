import json

# Define the paths to the files
file1_path = './appsettings.json'
file2_path = './local_appsettings.json'

# Load the contents of the files into variables
with open(file1_path) as file1:
    data1 = json.load(file1)

with open(file2_path) as file2:
    data2 = json.load(file2)

# Merge the two objects into a single dictionary
merged_data = {**data1, **data2}

# Write the merged data to a new JSON file
with open('appsettings.json', 'w') as merged_file:
    json.dump(merged_data, merged_file, indent=2)
