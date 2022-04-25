import matplotlib.pyplot as plt
import numpy as np
import sys

def drawPlot(coef, totalSpamElements):
    x = [x for x in range(0, totalSpamElements)]
    y = [f * coef for f in x]
    fig, ax = plt.subplots()
    ax.plot(x, y, linewidth=2.0)
    ax.set(xlim=(0, x[-1]), xticks=np.arange(1, x[-1]),
           ylim=(0, y[-1]), yticks=np.arange(1, x[-1]))
    plt.xticks([x[0], x[-1]], visible=True, rotation="horizontal")
    plt.yticks([y[0], y[-1], x[-1]], visible=True, rotation="horizontal")

    plt.show()


if __name__ == '__main__':
    print('Argument List:', str(sys.argv))

    coef = float(sys.argv[1].replace(',', '.'))
    totalSpamElements = int(sys.argv[2])
    drawPlot(coef, totalSpamElements)
