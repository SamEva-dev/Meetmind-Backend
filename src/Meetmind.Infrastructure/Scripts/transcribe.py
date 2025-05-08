import sys
import time

input_path = sys.argv[1]
output_path = sys.argv[2]

# Simulate processing
time.sleep(2)

with open(output_path, "w", encoding="utf-8") as f:
    f.write("Transcript generated for " + input_path)
