#!/bin/bash

# Find and remove _snapshots folders recursively
find . -type d -name "_snapshots" -exec rm -rf {} +

