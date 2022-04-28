# This is a sample Python script.
import math
import numpy as np
import matplotlib.pyplot as plt
import plotly.figure_factory as ff
import random


def gen_сorr_rand_data() -> [[]]:
    data = [[random.random() * 2.0, random.random() * 2.0] for _ in range(410)]
    return data


def flatter_coords(coords):
    x = []
    y = []

    for m_x, m_y in coords:
        x.append(m_x)
        y.append(m_y)
    return [x, y]


def calculate_neighbours(cluster_centers, all_coords):
    cluster_data = [[] for i in range(0, 3)]
    for coord in all_coords:
        min_distance = 100000
        cluster_index = 0
        current_index = 0
        for c_s in cluster_centers:
            distance = math.sqrt(math.pow(c_s[0] - coord[0], 2) + math.pow(c_s[1] - coord[1], 2))
            if distance < min_distance:
                min_distance = distance
                cluster_index = current_index
            current_index += 1

        cluster_data[cluster_index].append(coord)
    return cluster_data


def calculate_deviation(all_c_s):
    all_c_s_mod = []
    N = len(all_c_s)

    c_s_means = [0, 0, 0]
    c_s_dev = [0, 0, 0]

    for c_s in all_c_s:
        c_s_mod = []
        i = 0
        for c in c_s:
            mod = math.sqrt(math.pow(c[0], 2) + math.pow(c[1], 2))
            c_s_mod.append(mod)
            c_s_means[i] += mod
            i += 1

        all_c_s_mod.append(c_s_mod)

    k = 0
    for indx, m in enumerate(c_s_means):
        print(f"Before: {c_s_means[indx]}")
        c_s_means[indx] = m / N
        print(f"After: {m / N}")

    t = [0, 0, 0]

    for i in range(0, len(all_c_s_mod)):
        for j in range(0, len(all_c_s_mod[i])):
            t[j] += math.pow(all_c_s_mod[i][j] - c_s_means[j], 2)

    for i in range(0, len(t)):
        c_s_dev[i] = math.sqrt(t[i] / c_s_means[i])

    return c_s_dev


def calc_condition_of_clustering(c_s_dev, iter_count) -> bool:
    if iter_count < 3:
        return True
    print(c_s_dev)

    if math.fsum(c_s_dev)/len(c_s_dev) > .55:
        return True
    return False


def main():
    coords = gen_сorr_rand_data()

    c_s_indexes = []
    c_s = []
    all_c_s = []

    while len(c_s_indexes) < 3:
        rand = random.randrange(0, 410)
        if rand in c_s_indexes:
            continue
        c_s_indexes.append(rand)
        c_s.append(coords[rand])

    all_c_s.append(c_s)
    cluster_data = calculate_neighbours(c_s, coords)
    c_s_dev = calculate_deviation(all_c_s)

    iter_count = 0
    while calc_condition_of_clustering(c_s_dev, iter_count):
        iter_count += 1
        c_s = []

        for data in cluster_data:

            avgX = .0
            avgY = .0
            for coord in data:
                avgX += coord[0]
                avgY += coord[1]
            avgX /= len(data)
            avgY /= len(data)

            c_s.append([avgX, avgY])

        all_c_s.append(c_s)
        cluster_data = calculate_neighbours(c_s, coords)
        c_s_dev = calculate_deviation(all_c_s)

    cluster_data_flat = [flatter_coords(c) for c in cluster_data]

    for data in cluster_data_flat:
        plt.scatter(data[0], data[1], s=4)

    plt.show()

    x = [[m] for m, p in coords]

    X = np.array(x)
    fig1 = ff.create_dendrogram(X, orientation='left')
    fig1.update_layout(width=800, height=800)
    fig1.show()


# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    main()
