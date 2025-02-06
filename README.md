# Configuration
The settings of the Threaded Prime Number Search are configurable by editing the config.txt file. These include the number of **threads[x]**, the **given value[y]** that will be searched up to, the **printing variation**, and the **task division scheme**.

## Configuration Settings

- **x**: Number of threads used for the search.
  - Example: `x=10`

- **y**: The upper bound value that will be searched for prime numbers.
  - Example: `y=10000`

- **print**: Defines the printing variation for results.
  - Possible Values:
    - `immediate`: Print results immediately after finding a prime number.
    - `wait`: Wait for all threads to complete before printing results.
  - Example: `print=immediate`

- **division**: Specifies how the search task is divided among threads.
  - Possible Values:
    - `straight`: The numbers to be searched are divided into different ranges per thread (e.g., for a range of 1-1000 with 4 threads, the division will be 1-250, 251-500, etc.).
    - `linear`: The search is linear, but threads handle divisibility testing for individual numbers.
  - Example: `division=linear`

## Sample config.txt
```
x=4
y=1000
print=immediate
division=linear
```
