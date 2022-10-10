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

    # applying smoothing technique, default is method 4
    smoothing_enabled = True

    # retrieve data by MR group
    data, num_usable_data, num_total_data = get_data_by_mr(
        filename=filename, sheetname=sheetname, number_of_mrs=number_of_mrs)
    print(f"Total Number of Data: {num_total_data}")
    print(f"Total Number of Usable Data: {num_usable_data}")

    # calcualate BLEU score for each MR group
    result_by_mr = {}
    i = 1
    for each_data in data:
        result_by_mr.update({f"MR{i}": calculate_MR_bleu_score(
            each_data['Followup'], each_data['Result'], smoothing_enabled)})
        i = i+1

    # calculate average of each group and print average score
    # plot to see the relationships
    print("\nAverage BLEU Scores")
    for key in result_by_mr:
        average_score = statistics.mean(d for d in result_by_mr[key] if d is not None)
        print(f"Average BLEU score of {key}: {str(average_score)}")

    # plot data
    lineplot_data(results=result_by_mr)


# line plot
def lineplot_data(results):
    fig, ax = plt.subplots()

    # get range of graph
    longest_length = 0
    for key in results:
        if longest_length < len(results[key]):
            longest_length = len(results[key])
    xs = np.arange(longest_length)

    lines = []
    for key in results:
        series = np.array(results[key]).astype(np.double)
        mask = np.isfinite(series)
        newplot, = ax.plot(xs[mask], series[mask], linestyle='-', marker='o', visible=True, label=key)
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
    total_data = len(df)
    usable_data = 0

    # get data by MR
    for i in range(number_of_mrs):
        # get all MR data without blank (-) test cases (i.e. MR1)
        #mask = (df['MR'] == i+1) & (df['Followup'] != "-")
        mask = df['MR'] == i+1
        unused_mask = df['Followup'] != "-"
        current_mr_data = df.loc[mask]
        usable_data += len(df.loc[unused_mask])
        # append data to current_mr_data
        data_by_mr.append(current_mr_data)

    return data_by_mr, usable_data, total_data


# separate each bleu score by MRs
def calculate_MR_bleu_score(followups, results, smoothing_enabled):
    if isinstance(followups, pd.Series):
        followups = followups.array
    if isinstance(results, pd.Series):
        results = results.array

    bleu_scores = []

    # check length of the comparison lists
    if (followups.size != results.size):
        return -1

    for i in range(followups.size-1):
        if (followups[i] == "-"):
            bleu_scores.append(None)
        else:
            bleu_scores.append(bleu_score(
                reference=followups[i], candidate=results[i], smoothing_enabled=smoothing_enabled))

    return bleu_scores


# calculate bleu score of each sentence pair
def bleu_score(reference, candidate, smoothing_enabled):
    # default is smoothing technique 4 if enabled
    # default for n-gram is 1 n-gram
    # to change n-gram, simply change n_gram variable
    n_gram = 1

    # corresponding weights depending on the n-gram of bleu
    weight = {
        1: (1.0, 0, 0, 0),
        2: (0.5, 0.5, 0, 0),
        3: (0.33, 0.33, 0.33, 0),
        4: (0.25, 0.25, 0.25, 0.25),
    }

    smoothing = SmoothingFunction().method4 if smoothing_enabled else None
    return bleu(reference, candidate, weight[n_gram], smoothing_function=smoothing)


if __name__ == '__main__':
    main()
