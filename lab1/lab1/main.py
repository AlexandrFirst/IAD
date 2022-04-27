import matplotlib.pyplot as plt
import sys
import re

def drawPlot(x, y):
    fig, ax = plt.subplots()
    ax.plot(x, y, linewidth=2.0)
    plt.show()

if __name__ == '__main__':
    print('Argument List:', str(sys.argv))
    x = []
    y = []
    regex = "^accuracy: (\d+(?:\,\d+)?); count: (\d+)\\n$"
    i = 0
    with open('accuracy.txt') as f:
        lines = f.readlines()
        for line in lines:
            if i == 5:
                m = re.match(regex, line)
                y.append(m[1])
                x.append(m[2])
                i = 0
                continue
            i += 1
    drawPlot(x, y)
