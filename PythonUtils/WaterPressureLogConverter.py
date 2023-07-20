import os
import shutil
from datetime import datetime, time, timedelta

def get_time_index(time_str):
    dt = datetime.strptime(time_str, "%H:%M:%S")
    return (dt.hour * 60 + dt.minute) // 5 + 1

def list_files(directory, prefix):
    return [f for f in os.listdir(directory) if os.path.isfile(os.path.join(directory, f)) and str(f).startswith(prefix)]

def transform_pressure_files(source_path, destination_path):
    for file_name in list_files(source_path, "Pressure_"):
        print("Processing file: " + file_name)
        with open(source_path + file_name, mode="rt", encoding="utf-8") as tables_file:
            all_lines = tables_file.readlines()

        values = {"": [""]}
        date_time = datetime(2020, 1, 1, 0, 0, 0)

        for i in range(0, 12 * 24):
            values[""].append(date_time.strftime("%H:%M:%S"))
            date_time = date_time + timedelta(minutes = 5)

        for line in all_lines:
            if line.startswith("Time"):
                continue

            (date_val, time_val, value_val) = line.strip("\n").split(",")

            if date_val in values:
                values[date_val][get_time_index(time_val)] = value_val
            else:
                values[date_val] = [""] * len(values[""])
                values[date_val][get_time_index(time_val)] = value_val

        with open(destination_path + file_name, mode="wt", encoding="utf-8") as tables_file:
            for key in values:
                tables_file.write(str(key) + ",".join(values[key]) + "\n")

def copy_files_starting_with(source_path, destination_path, prefix):
    print(f"Copying {prefix}*.* files...")
    for file_name in list_files(source_path, prefix):
        shutil.copy2(source_path + file_name, destination_path + file_name)

source_path = "f:/"
destination_path = "d:/Androvil/Projects/Home Autoimation/"

if not os.path.exists(source_path):
    print("Source path does not exist: " + source_path)
    input("Press Enter to exit...")
    exit(1)

transform_pressure_files(source_path, destination_path)
copy_files_starting_with(source_path, destination_path, "Log_")
copy_files_starting_with(source_path, destination_path, "conf")

input("Press Enter to exit...")
