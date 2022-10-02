from cProfile import label
from multiprocessing import current_process
from unittest import result
from nltk.translate import bleu
from nltk.translate.bleu_score import SmoothingFunction
import pandas as pd
import numpy as np
import statistics as statistics
import matplotlib.pyplot as plt
from matplotlib.widgets import CheckButtons


def main():
    filename = r'full_dataset.xlsx'
    sheetname = r'dataset'
    number_of_mrs = 7

    # type of plot
    # True - line plot; False - scatter plot
    is_line_plot = False

    # retrieve data by MR group
    data = get_data_by_mr(
        filename=filename, sheetname=sheetname, number_of_mrs=number_of_mrs)

    # calcualate BLEU score for each MR group
    result_by_mr = {}
    i = 1
    for each_data in data:
        result_by_mr.update({f"MR{i}": calculate_MR_bleu_score(
            each_data['Followup'], each_data['Result'])})
        i = i+1

    # calculate average of each group and print average score
    # plot to see the relationships
    for key in result_by_mr:
        average_score = statistics.mean(result_by_mr[key])
        print(f"Average BLEU score of {key}: {str(average_score)}")

    # plot data
    lineplot_data(results=result_by_mr, is_line_plot=is_line_plot)


# line plot
def lineplot_data(results, is_line_plot=False):
    fig, ax = plt.subplots()
    plot_type = "" if is_line_plot else "o"

    lines = []
    for key in results:
        newplot, = ax.plot(results[key], plot_type, visible=True, label=key)
        ax.legend()
        lines.append(newplot)
    plt.subplots_adjust(left=0.2)

    # Make checkbuttons with all plotted lines with correct visibility
    rax = plt.axes([0.05, 0.4, 0.1, 0.15])
    labels = [str(line.get_label()) for line in lines]
    visibility = [line.get_visible() for line in lines]
    check = CheckButtons(rax, labels, visibility)

    def func(label):
        index = labels.index(label)
        lines[index].set_visible(not lines[index].get_visible())
        plt.draw()

    check.on_clicked(func)
    plt.show()


# retrieve data from file separating them into MR groups
def get_data_by_mr(filename, sheetname, number_of_mrs=0):
    df = pd.read_excel(filename, sheetname)
    data_by_mr = []

    # get data by MR
    for i in range(number_of_mrs):
        # get all MR data without blank (-) test cases (i.e. MR1)
        mask = (df['MR'] == i+1) & (df['Followup'] != "-")
        current_mr_data = df.loc[mask]
        # append data to current_mr_data
        data_by_mr.append(current_mr_data)

    return data_by_mr


# separate each bleu score by MRs
def calculate_MR_bleu_score(followups, results):
    if isinstance(followups, pd.Series):
        followups = followups.array
    if isinstance(results, pd.Series):
        results = results.array

    bleu_scores = []

    # check length of the comparison lists
    if (followups.size != results.size):
        return -1

    for i in range(followups.size-1):
        bleu_scores.append(bleu_score(
            reference=followups[i], candidate=results[i]))

    return bleu_scores


# calculate bleu score of each sentence pair
def bleu_score(reference, candidate):
    # smoothing = SmoothingFunction().method1
    return bleu(reference, candidate, (1,))


if __name__ == '__main__':
    main()
